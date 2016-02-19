namespace WaveAnalysisProject
{
    //class for Complex numbers
    //has a real and imaginary portion
    public class Complex
    {
        //declarations of private members
        private double re;
        private double im;

        //Constructor for Complex class
        public Complex(double r, double i)
        {
            re = r;
            im = i;
        }

        //Setter for real portion
        public void setReal(double r)
        {
            re = r;
        }

        //Getter for real portion
        public double getReal()
        {
            return re;
        }

        //Setter for imaginary portion
        public void setImm(double i)
        {
            im = i;
        }

        //Getter for imaginary portion
        public double getImm()
        {
            return im;
        }
    }
}
