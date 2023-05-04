using System;
using ArrayOps;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace ConventionalAudio {
    public class PeakCounter {
        // number of peaks to detect
        private int num_peaks;

        // filter function to run before peak counting
        private Func<float[], float[]> func;

        // interval between peaks
        private int skipInterval;

        // peak indices and values
        private float[] m_amp;
        private int[] m;
        private float[] sorted;

        public PeakCounter(int num_peaks, Func<float[], float[]> func=null, int skipInterval=4000) {
            this.num_peaks = num_peaks;
            this.func = func;
            if (this.func == null) {
                this.func = DefaultFilter.DefaultFilterFunction;
            }
            this.skipInterval = skipInterval;
            this.m_amp = new float[7];
            this.m = new int[7];
            this.sorted = new float[7];
        }

        public bool countPeaks(float[] data) {
            // run function on input
            float[] filtered = this.func(data);

            // peak finding procedure:
            // NOTE: 0.25 * 3 seconds = this.skipInterval samples b/c sample rate is 16000
            // first -> max of array
            // second -> max to left of (first - 0.25 * 3 seconds)
            // third -> max to right of (first + 0.25 * 3 seconds)
            // fourth -> max to left of (second - 0.25 * 3 seconds)
            // fifth -> between second and first (second + 0.25 * 3 seconds -> first - 0.25 * 3 seconds)
            // sixth -> between first and third (first + 0.25 * 3 seconds -> third - 0.25 * 3 seconds)
            // seventh -> right to third of (third + 0.25 * 3 seconds)

            m[0] = Ops.argmax(filtered);
            m[1] = Ops.argmax(filtered, 0, Math.Max(0, m[0] - this.skipInterval));
            m[2] = Ops.argmax(filtered, Math.Min(m[0] + this.skipInterval, data.Length - 1), data.Length);
            m[3] = Ops.argmax(filtered, 0, Math.Max(0, m[1] - this.skipInterval));
            m[4] = -1;
            m[5] = -1;

            if (m[0] - m[1] < 1) {
                m[4] = Ops.argmax(filtered,  Math.Min(m[1] + this.skipInterval, data.Length - 1), Math.Max(0, m[0] - this.skipInterval));
            }

            if (m[2] - m[0] < 1) {
                m[5] = Ops.argmax(filtered,  Math.Min(m[0] + this.skipInterval, data.Length - 1), Math.Max(0, m[2] - this.skipInterval));
            }

            m[6] = Ops.argmax(filtered, Math.Min(m[2] + this.skipInterval, data.Length - 1), data.Length);

            // convert indices to values
            for (int i=0; i<7; i++) {
                int idx = m[i];
                if (idx != -1) {
                    m_amp[i] = filtered[idx];
                } else {m_amp[i] = 0;} // if m[4] or m[5] are negative make sure they are at the end when sorted
            }

            // copy values
            Array.Copy(m_amp, sorted, m_amp.Length);

            // sort values (largest -> smallest)
            Ops.qsortr(sorted, 0, sorted.Length-1);

            int clapCount = 1;
            
            // peak comparison procedure
            for (int i=2; i<6; i++) {
                // compute the average of the i largest peaks
                float avgMax = 0;
                for (int j=0; j<i; j++) {
                    avgMax += sorted[i];
                }
                avgMax /= i;

                // find the smallest amplitude of unsorted peaks
                float minAmp = m_amp[0];
                for (int j=0; j<i; j++) {
                    if (m_amp[i] < minAmp) {
                        minAmp = m_amp[i];
                    }
                }

                // Debug.Log("minimum: " + minAmp + " threshold: " + 0.7*avgMax);

                // if the smallest amplitude is greater than 0.7 * average of the i largest peaks increment clap count
                if (minAmp > (0.7 * avgMax)) {
                    clapCount++;
                } else {break;}
            }

            Debug.Log(clapCount);
            bool detected = (clapCount == this.num_peaks);

            return detected;
        }
    }

    public static class DefaultFilter {
        // butterworth filters
        private static FilterButterworth hp = new FilterButterworth(600, 16000, 
            FilterButterworth.PassType.Highpass, DefaultParams.resonance);
        private static FilterButterworth lp = new FilterButterworth(3000, 16000, 
            FilterButterworth.PassType.Lowpass, DefaultParams.resonance);

        public static float[] DefaultFilterFunction(float[] data) {
            // allocate output
            float[] filtered = new float[data.Length];

            // copy over input
            Array.Copy(data, 0, filtered, 0, data.Length);

            // initialize high and lowpass butterworth filters
            hp.filterInit(filtered[0], filtered[1]);
            lp.filterInit(filtered[0], filtered[1]);

            // high pass at 600hz and next low pass at 3000hz
            for (int i=2; i<data.Length; i++) {
                filtered[i] = hp.Update(filtered[i]);
                filtered[i] = lp.Update(filtered[i]);
            }

            // run a 5-point moving average
            filtered = MovingAverage.run(filtered, 5);

            return filtered;
        }

    }
}