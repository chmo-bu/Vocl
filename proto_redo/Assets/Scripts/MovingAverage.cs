using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovingAverage {
    public static class MovingAverage
    {
        public static float[] run(float[] data, int period) {
            float[] buffer = new float[period];
            float[] output = new float[data.Length];
            int current_index = 0;
            for (int i=0; i<data.Length; i++) {
                buffer[current_index] = data[i]/period;
                float ma = 0;
                for (int j=0; j<period; j++)
                    {
                        ma += buffer[j];
                    }
                output[i] = ma;
                current_index = (current_index + 1) % period;
            }
            return output;
        }
    }
}