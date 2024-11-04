namespace BmTest.Helpers;

public static class MathHelpers
{
    /*
    * Half = Float16
    * http://www.openexr.com/  source: ilmbase-*.tar.gz/Half/toFloat.cpp
    * http://en.wikipedia.org/wiki/Half_precision
    * Also look at GL_ARB_half_float_pixel
    */
    public static unsafe float HalfToFloat(this ushort half)
    {
        float f = 0;
        int sign = (half >> 15) & 0x00000001;
        int exp = (half >> 10) & 0x0000001F;
        int mant = half & 0x000003FF;

        exp += 127 - 15;
        *(uint*)&f = ((uint)sign << 31) | ((uint)exp << 23) | ((uint)mant << 13);
        return f;
    }
}
