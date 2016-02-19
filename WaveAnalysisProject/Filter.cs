using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveAnalysisProject
{
    //Helper class
    //Creation of filters
    class Filter
    {   
        /*
            create a low pass filter based on user selection
            user selects a frequency cutoff
            all frequencies after freq cutoff are removed

            freq = (f(frequency bins) * SamplingRate) / NumSamples

                                                      Nyquist
            [1, 1, 1, 1, 1, 1,            0, 0, 0, 0,    0,    0, 0, 0, 0, 1, 1, 1, 1, 1]
                            freq(cutoff) ---------------> <---------------
        */
        public Complex[] createLowPassFilter(int selStart, int selEnd, double[] wav)
        {
            //error checking
            if (selStart != selEnd)
            {
                return null;
            }
            int numSamples = wav.Length;
            Complex[] filter = new Complex[numSamples];
            int nyquistLimit = numSamples / 2;
            int amtZeroes = ((nyquistLimit - selStart) * 2) + 1;
            int i;

            //beginning of filter
            for(i = 0; i <= selStart; i++)
            {
                filter[i] = new Complex(1,1);
            }

            //middle of filter
            for(; i <= selStart + amtZeroes; i++)
            {
                filter[i] = new Complex(0,0);
            }

            //end of filter
            for(; i < numSamples; i++)
            {
                filter[i] = new Complex(1,1);
            }

            return filter;
        }
    }
}
