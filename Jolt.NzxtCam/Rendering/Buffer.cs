using System.Buffers;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;
using static Jolt.NzxtCam.MathF;
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

public static class TriangleRenderer
{
    public static void Render(CBuffer cbuffer, ZBuffer zbuffer, Vector4 p0, Vector4 p1, Vector4 p2, int color) {
        //
        int I(float f) => (int)(f + 0.5f);
        float F(int i) => (float)i;

        // Sort
        var i0 = p1.Y < p0.Y
            ? p2.Y < p1.Y ? 2 : 1
            : p2.Y < p0.Y ? 2 : 0;
        if (i0 == 1) (p0, p1, p2) = (p1, p2, p0);
        else if (i0 == 2) (p0, p1, p2) = (p2, p0, p1);

        // TODO: Perhaps clip?

        // Guard clauses.
        var (w, h) = (cbuffer.Size.Width, cbuffer.Size.Height);
        var (y0, y1, y2) = (I(p0.Y), I(p1.Y), I(p2.Y));
        // Top.
        if (y0 < 0) {
            throw new ArgumentOutOfRangeException();
        }
        // Bottom.
        if (y1 > h || y2 > h) {
            throw new ArgumentOutOfRangeException();
        }
        // Left.
        if (I(p0.X) < 0 | I(p1.X) < 0 || I(p2.X) < 0) {
            throw new ArgumentOutOfRangeException();
        }
        // Right.
        if (I(p0.X) > w | I(p1.X) > w || I(p2.X) > w) {
            throw new ArgumentOutOfRangeException();
        }

        // "split"
        Vector4 p3;
        int y3;
        if (y1 < y2) {
            var t = F(y1 - y0) / (y2 - y0);
            p3 = Vector4.Lerp(p0, p2, t);
            y3 = I(p3.Y);
        } else {
            p3 = p2; y3 = y2;
            p2 = p1; y2 = y1;
            var t = F(y3 - y0) / (y2 - y0);
            p1 = Vector4.Lerp(p0, p2, t);
            y1 = I(p1.Y);
        }

        //
        Render(cbuffer, zbuffer, p0, p0, p3, p1, color);
        Render(cbuffer, zbuffer, p3, p1, p2, p2, color);
    }

    public static void Render(CBuffer cbuffer, ZBuffer zbuffer, Vector4 upperLeft, Vector4 upperRight, Vector4 lowerLeft, Vector4 lowerRight, int color) {
        int I(float f) => (int)(f + 0.5f);
        float F(int i) => (float)i;
        var y0 = I(upperLeft.Y);
        var yn = I(lowerLeft.Y);
        var dy = yn - y0;
        for (int y = y0; y < yn; y++) {
            var t = F(y - y0) / dy;
            var left = Vector4.Lerp(upperLeft, lowerLeft, t);
            var right = Vector4.Lerp(upperRight, lowerRight, t);
            
            var x0 = I(left.X);
            var xn = I(right.X);
            var dx = xn - x0;
            for (var x = x0; x < xn; x++) {
                t = F(x - x0) / dx;
                var z = Lerp(left.Z, right.Z, t);
                if (z < zbuffer[x, y]) {
                    zbuffer[x, y] = z;
                    //var c = (int)(255 * Saturate(z/500));
                    //color = Color.FromArgb(255, c, c, c).ToArgb();
                    cbuffer[x, y] = color;
                }
            }
        }
    }
}