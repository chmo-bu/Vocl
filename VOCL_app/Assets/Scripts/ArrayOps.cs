using System;

namespace ArrayOps {

    public static class Ops{
        public static int argmax(float[] arr) {
            float ma = arr[0];
            int amax = 0;
            for (int i=1; i<arr.Length; i++) {
                float temp = arr[i];
                if (temp > ma) {
                    amax = i;
                    ma = temp;
                }
            }
            return amax;
        }

        public static int argmax(float[] arr, int start, int end) {
            float ma = arr[start];
            int amax = start;
            for (int i=start+1; i<end; i++) {
                float temp = arr[i];
                if (temp > ma) {
                    amax = i;
                    ma = temp;
                }
            }
            return amax;
        }

        public static void qsortr(float[] arr, int l, int h) {
            // Create an auxiliary stack
            int[] stack = new int[h - l + 1];

            // initialize top of stack
            int top = -1;

            // push initial values of l and h to
            // stack
            stack[++top] = l;
            stack[++top] = h;

            // Keep popping from stack while
            // is not empty
            while (top >= 0) {
                // Pop h and l
                h = stack[top--];
                l = stack[top--];

                // Set pivot element at its
                // correct position in
                // sorted array
                int p = partition(arr, l, h);

                // If there are elements on
                // left side of pivot, then
                // push left side to stack
                if (p - 1 > l) {
                    stack[++top] = l;
                    stack[++top] = p - 1;
                }

                // If there are elements on
                // right side of pivot, then
                // push right side to stack
                if (p + 1 < h) {
                    stack[++top] = p + 1;
                    stack[++top] = h;
                }
            }
        }

        private static int partition(float[] arr, int low, int high) {
            float temp;
            float pivot = arr[high];

            // index of larger element
            int i = (low - 1);
            for (int j = low; j <= high - 1; j++) {
                // If current element is greater
                // than or equal to pivot
                if (arr[j] >= pivot) {
                    i++;

                    // swap arr[i] and arr[j]
                    temp = arr[i];
                    arr[i] = arr[j];
                    arr[j] = temp;
                }
            }

            // swap arr[i+1] and arr[high]
            // (or pivot)

            temp = arr[i + 1];
            arr[i + 1] = arr[high];
            arr[high] = temp;

            return i + 1;
        }
    }
}