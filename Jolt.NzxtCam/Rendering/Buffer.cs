using System.Buffers;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;
namespace Jolt.NzxtCam;

public unsafe class Buffer<T> : IDisposable where T : struct {
    private readonly MemoryHandle handle;
    private readonly IMemoryOwner<T> owner;
    private const int CellWidth = 4;
    private const int CellHeight = 4;
    private const int CellSize = CellWidth * CellHeight;
    private static readonly int TypeSize = Marshal.SizeOf<T>();
    private static readonly int CellByteCount = CellSize * TypeSize;
    private bool isDisposed;

    public Memory<T> Memory { get; private set; }
    public Size Size { get; }
    public int ColumnCount { get; }
    public int RowStride { get; } // Number of T per row.

    public Buffer(Size size) {
        var w = (size.Width + 15) & ~15;
        var h = size.Height;
        var padding = CellByteCount;
        var itemCount = w * h + padding;

        //
        Size = size;
        ColumnCount = w / CellWidth;
        RowStride = ColumnCount * CellSize;
        this.owner = MemoryPool<T>.Shared.Rent(itemCount);
        this.handle = owner.Memory.Pin();

        //
        var byteOffset = CellByteCount - (int)((ulong)handle.Pointer % (ulong)CellByteCount);
        var itemOffset = byteOffset / TypeSize;
        Memory = owner.Memory[itemOffset..];
    }

    public void Clear() {
        Memory.Span.Clear();
    }

    public void Fill(T value) {
        Memory.Span.Fill(value);
    }

    protected int CalculateIndex(int x, int y) {
        var (rowIndex, columnIndex, cellIndex) = CalculateIndices(x, y);
        return rowIndex * RowStride + columnIndex * CellSize + cellIndex;
    }

    protected (int RowIndex, int ColumnIndex, int CellIndex) CalculateIndices(int x, int y) {
        var rowIndex = y / CellHeight;
        var columnIndex = x / CellWidth;
        var cellIndex = 
            (y % CellHeight) * CellWidth + 
            (x % CellWidth);
        return (rowIndex, columnIndex, cellIndex);
    }

    public T this[int x, int y] {
        set => Memory.Span[CalculateIndex(x, y)] = value;
        get => Memory.Span[CalculateIndex(x, y)];
    }

    protected virtual void Dispose(bool isDisposing) {
        if (isDisposed) {
            return;
        }

        // Dispose managed objects.
        if (isDisposing) {
            owner?.Dispose();
        }

        // Dispose unmanaged objects.
        isDisposed = true;
    }

    // // TODO: override finalizer only if 'Dispose(bool isDisposing)' has code to free unmanaged resources
    // ~Buffer()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose() {
        // Do not change this code. Put cleanup code in the 'Dispose(bool isDisposing)' method.
        Dispose(isDisposing: true);
        GC.SuppressFinalize(this);
    }
}

public unsafe class ZBuffer : Buffer<float> {
    public ZBuffer(Size size) : base(size) { 
    }
}

public unsafe class NBuffer : Buffer<Vector2> {
    public NBuffer(Size size) : base(size) { 
    }
}

public unsafe class CBuffer : Buffer<int> {
    private Bitmap? bitmap;

    public CBuffer(Size size) : base(size) { 
    }

    protected override void Dispose(bool isDisposing) {
        base.Dispose(isDisposing);

        // Dispose managed objects.
        if (isDisposing) {
            bitmap?.Dispose();
        }
    }

    public Bitmap UpdateBitmap() {
        var (w, h) = (Size.Width, Size.Height);
        // TODO: The bitmap memory should probably be manually allocated to ensure memory alignment.
        bitmap ??= new Bitmap(w, h, PixelFormat.Format32bppArgb);
        var bitmapData = bitmap.LockBits(new(0, 0, w, h), ImageLockMode.WriteOnly, bitmap.PixelFormat);
        try {
            var line0 = (byte*)(void*)bitmapData.Scan0;
            for (int y = 0; y < h; y++) {
                var line = (int*)&line0[y * bitmapData.Stride];
                for (int x = 0; x < w; x++) {
                    line[x] = this[x, y];
                }
            }
        }
        finally {
            bitmap.UnlockBits(bitmapData);
        }
        return bitmap;
    }
}
