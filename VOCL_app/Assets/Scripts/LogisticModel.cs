using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

namespace Models {
    public class LogisticModel
    {
        // PCA variables
        private float[/*num_components*/,/*feature_size*/] components;
        private float[/*feature_size*/] mean;
        private int num_components;

        // size of input
        private int feature_size;
        
        // logistic regression variables
        private float[/*num_components*/,/*1*/] weights;
        private float bias;

        public LogisticModel(int feature_size, int num_components) {//, string pca_path, string logit_path) {
            // assign dimension parameters
            this.num_components = num_components;
            this.feature_size = feature_size;

            // allocate weights and component
            components = new float[num_components, feature_size];
            weights = new float[num_components, 1];
            mean = new float[feature_size];

            // read eigen vectors
            using (BinaryReader br = new BinaryReader(
                File.Open("Assets/Resources/pca_components32.bin", 
                FileMode.Open))) {
                    for (int i=0; i<num_components; i++) {
                        for (int j=0; j<feature_size; j++) {
                            components[i, j] = br.ReadSingle();
                        }
                    }
            }

            using (BinaryReader br = new BinaryReader(
                File.Open("Assets/Resources/pca_mean32.bin", 
                FileMode.Open))) {
                    for (int i=0; i<feature_size; i++) {
                        mean[i] = br.ReadSingle();
                    }
            }

            using (BinaryReader br = new BinaryReader(
                File.Open("Assets/Resources/logistic_coef32.bin", 
                FileMode.Open))) {
                    for (int i=0; i<num_components; i++) {
                        weights[i, 0] = br.ReadSingle();
                    }
            }

            using (BinaryReader br = new BinaryReader(
                File.Open("Assets/Resources/logistic_intercept32.bin", 
                FileMode.Open))) {
                    bias = br.ReadSingle();
            }

            // // Debug.Log("components = " + String.Join(",",
            // //         new List<float>(components)
            // //         .ConvertAll(i => i.ToString())
            // //         .ToArray()));


            // // load logistic weights and biases

            // // weights
            // TextAsset coefs = Resources.Load("logit_coefs32.bin") as TextAsset;
            // Buffer.BlockCopy(weights, 0, coefs.bytes, 0, num_components);

            // // bias
            // TextAsset intercept = Resources.Load("logit_intercept32.bin") as TextAsset;
            // bias = System.BitConverter.ToSingle(intercept.bytes, 0);

            // // Debug.Log("coefs = " + String.Join(",",
            // //         new List<float>(weights)
            // //         .ConvertAll(i => i.ToString())
            // //         .ToArray()));

            // Debug.Log("intercept = " + bias);
        }

        public float[,] predict(float[,] x) {
            // perform pca
            float[,] output = pca(centerData(x));

            // multiply weights
            output = matmul(output, weights);

            // add bias
            output = matAdd(output, bias);

            // get logits
            output = sigmoid(output);

            // get prediction
            // output = matGT(output, (float)0.5);

            return output;
        }

        private float[,] pca(float[/*m*/,/*num_features*/] x) {
            // multiply features and top eigenvectors
            float[,] output = matmul(x, transpose(components));

            return output;
        }

        private float[,] sigmoid(float[/*m*/,/*1*/] x) {
            // get dimensions
            int m = x.GetLength(0);

            // allocate output
            float[,] output = new float[m, 1];

            // apply sigmoid
            for (int i=0; i<m; i++) {
                output[i, 0] = 1 / (1 + MathF.Exp(-x[i, 0]));
            }

            return output;
        }

        private float[,] matmul(float[/*m*/,/*n*/] mat1, float[/*n*/,/*d*/] mat2) {
            // get dims
            int m = mat1.GetLength(0);
            int d = mat2.GetLength(1);
            int r = mat1.GetLength(1);

            // output array
            float[,] output = new float[m, d];

            // check if dimensions compatible
            if (r != mat2.GetLength(0)) {
                return null;
            }

            // perform matrix multiplication
            int i, j, k;
            for (i = 0; i < m; i++) {
                for (j = 0; j < d; j++) {
                    output[i, j] = 0;
                    for (k = 0; k < r; k++)
                        output[i, j] += mat1[i, k] * mat2[k, j];
                }
            }

            return output;
        }

        private float[/*n*/,/*m*/] transpose(float[/*m*/,/*n*/] x) {
            // get dimensions of matrix
            int m = x.GetLength(0);
            int n = x.GetLength(1);

            // allocate output
            float[,] output = new float[n, m];

            // transpose
            int i, j;
            for (i = 0; i < n; i++) {
                for (j = 0; j < m; j++) {
                    output[i,j] = x[j,i];
                }
            }

            return output;
        }

        private float[/*n*/,/*m*/] matAdd(float[/*m*/,/*n*/] x, float scalar) {
            // get dimensions of matrix
            int m = x.GetLength(0);
            int n = x.GetLength(1);

            // allocate output
            float[,] output = new float[m, n];

            // transpose
            int i, j;
            for (i = 0; i < m; i++) {
                for (j = 0; j < n; j++) {
                    output[i, j] = x[i, j] + scalar;
                }
            }

            return output;
        }

        private float[/*n*/,/*m*/] matGT(float[/*m*/,/*n*/] x, float scalar) {
            // get dimensions of matrix
            int m = x.GetLength(0);
            int n = x.GetLength(1);

            // allocate output
            float[,] output = new float[m, n];

            // transpose
            int i, j;
            for (i = 0; i < m; i++) {
                for (j = 0; j < n; j++) {
                    output[i, j] = (float)(Convert.ToInt32(x[i, j] > scalar));
                }
            }

            return output;
        }

        private float[,] centerData(float[/*m*/,/*n*/] x) {
            // get dimensions of matrix
            int m = x.GetLength(0);
            int n = x.GetLength(1);

            float[,] output = new float[m, n];

            int i, j;
            for (i=0; i<m; i++) {
                for (j=0; j<n; j++) {
                    output[i, j] = x[i, j] - mean[j];
                }
            }
            return output;
        }
    }
}