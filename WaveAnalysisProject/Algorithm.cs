using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WaveAnalysisProject
{
    //Helper class
    //Contains algorithms to be used
    class Algorithm
    {
        //Perform DFT on samples, get complex values, get amplitude of frequency bin
        public double[] performDFT(double[] samples)
        {
            Complex[] DFTValues;
            double[] result = new double[samples.Length];

            //Obtain Complex values by performing DFT
            DFTValues = DFT(samples);
            for(int t = 0; t < result.Length; t++)
            {
                result[t] = getAmp(DFTValues[t]);
            }

            return result;
        }
        
        //Full fourier
        //Af = 1/N SUM(t=0 .. N-1)[ Samples(t) * (cos(2PI t f / N) - i sin(2PI t f / N)]
        public Complex[] DFT(double[] samples)
        {
            Complex[] A = new Complex[samples.Length];
            double N = samples.Length;
            for (int f = 0; f < N; f++)
            {
                double real = 0;
                double imm = 0;
                for (int t = 0; t < N; t++)
                {
                    real += samples[t] * Math.Cos(2 * Math.PI * t * f / N);
                    imm -= samples[t] * Math.Sin(2 * Math.PI * t * f / N);
                }
                A[f] = new Complex(real, imm);
            }
            return A;
        }

        //Return amplitude using pythag sqrt(real*real + im*im)
        public double getAmp(Complex num)
        {
            return Math.Sqrt(Math.Pow(num.getReal(), 2) + Math.Pow(num.getImm(), 2));
        }

        //Reproduce the wave from the DFT vlaues
        public double[] ReverseDFT(Complex[] A)
        { 
            int n = A.Length;
            double[] wave = new double[n];
            Complex temp;
            double arg;
            for(int t = 0; t < n; t++)
            {
                arg = 2 * Math.PI * t / n;
                temp = new Complex(0, 0);
                double real = 0;
                double imma = 0;
                for(int f = 0; f < n; f++)
                {
                    real += A[f].getReal() * Math.Cos(arg * f);
                    imma += A[f].getImm() * Math.Sin(arg * f); 
                }
                wave[t] = (real - imma) / n;
            }            
            return wave;
        }

        //Convolution algorithm to create new samples
        public double[] Convolve(double[] weights, double[] samples)
        {
            double[] newSamples = new double[samples.Length];
            double sum = 0;

            for(int i = 0; i < newSamples.Length; i++)
            {
                for(int j = 0; j < weights.Length; j++)
                {
                    if (i + j < samples.Length)
                    {
                        sum += weights[j] * samples[i + j];
                    } else {
                        sum += 0; //pad with 0's
                    }
                }
                newSamples[i] = sum;
                sum = 0;
            }

            return newSamples;
        }

        //Filtering using convolution to the time domain
        public double[] Convolution(Complex[] filter, double[] samples)
        {
            double[] weights = ReverseDFT(filter);
            return Convolve(weights, samples);
        }

        //apply triangle windowing on samples
        public double[] TriangleWindow(double[] samples)
        {
            int N = samples.Length;
            double[] weights = new double[N];
            double[] dummies = new double[N];

            for (int n = 0; n < N; n++)
            {
                weights[n] = 1 - Math.Abs((n - (N - 1)/2) / (N/2));
                dummies[n] = samples[n] * weights[n];
            }
            return dummies;
        }

        //apply Welch windowing oo samples
        public double[] WelchWindow(double[] samples)
        {
            int N = samples.Length;
            double[] weights = new double[N];
            double[] dummies = new double[N];

            for (int n = 0; n < N; n++)
            {
                weights[n] = 1 - (Math.Pow(((n- ((N-1) / 2)) / ((N-1)/2)), 2));
                dummies[n] = samples[n] * weights[n];
            }
            return dummies;
        }

        //apply hanning windowing on samples
        public double[] HanningWindow(double[] samples)
        {
            int N = samples.Length;
            double[] weights = new double[N];
            double[] dummies = new double[N];

            for (int n = 0; n < N; n++)
            {
                weights[n] = 0.5 * (1 - Math.Cos((2 * Math.PI * n)/(N-1)));
                dummies[n] = samples[n] * weights[n];
            }
            return dummies;
        }
    }
}
