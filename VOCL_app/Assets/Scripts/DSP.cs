using System;
using UnityEngine;
using System.Numerics;

namespace DSP {

    // public static class FFT {
    //     public static void CalculateFFT(Complex[] samples, float[] result, bool reverse)
    //     {
    //         int power = (int)Math.Log(samples.Length, 2);
    //         int count = 1;
    //         for (int i = 0; i < power; i++)
    //             count <<= 1;

    //         int mid = count >> 1; // mid = count / 2;
    //         int j = 0;
    //         for (int i = 0; i < count - 1; i++)
    //         {
    //             if (i < j)
    //             {
    //                 var tmp = samples[i];
    //                 samples[i] = samples[j];
    //                 samples[j] = tmp;
    //             }
    //             int k = mid;
    //             while (k <= j)
    //             {
    //                 j -= k;
    //                 k >>= 1;
    //             }
    //             j += k;
    //         }
    //         Complex r = new Complex(-1, 0);
    //         int l2 = 1;
    //         for (int l = 0; l < power; l++)
    //         {
    //             int l1 = l2;
    //             l2 <<= 1;
    //             Complex r2 = new Complex(1, 0);
    //             for (int n = 0; n < l1; n++)
    //             {
    //                 for (int i = n; i < count; i += l2)
    //                 {
    //                     int i1 = i + l1;
    //                     Complex tmp = r2 * samples[i1];
    //                     samples[i1] = samples[i] - tmp;
    //                     samples[i] += tmp;
    //                 }
    //                 r2 = r2 * r;
    //             }
    //             r.Imaginary = Math.Sqrt((1d - r.Real) / 2d);
    //             if (!reverse)
    //                 r.Imaginary = -r.Imaginary;
    //             r.Real = Math.Sqrt((1d + r.Real) / 2d);
    //         }
    //         if (!reverse)
    //         {
    //             double scale = 1d / count;
    //             for (int i = 0; i < count; i++)
    //                 samples[i] *= scale;
    //             for (int i = 0; i < samples.Length / 2; i++)
    //             {
    //                 result[i] = (float)samples[i].Magnitude;
    //             }
    //         }
    //         else
    //         {
    //             for (int i = 0; i < samples.Length / 2; i++)
    //             {
    //                 result[i] = (float)(Math.Sign(samples[i].Real) * samples[i].Magnitude);
    //             }
    //         }
    //     }

    //     public static void Float2Complex(float[] input, Complex[] result)
    //     {
    //         for (int i = 0; i < input.Length; i++)
    //         {
    //             result[i] = new Complex(input[i], 0);
    //         }
    //     }
    // }
}