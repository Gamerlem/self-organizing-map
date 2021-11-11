using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;

namespace SOFM
{
    public delegate void EndEpochEventHandler(object sender, EndEpochEventArgs e);
    public delegate void EndIterationEventHandler(object sender, EventArgs e);

    public class NeuralNetwork
    {
        private Neuron[,] outputLayer;
        private Color[,] colorMatrix;

        public Neuron[,] OutputLayer
        {
            get { return outputLayer; }
            set { outputLayer = value; }
        }
        private int inputLayerDimension;
        private int outputLayerDimension;
        private int numberOfPatterns;
        private List<List<double>> patterns;
        private List<string> classes;
        private SortedList<string, int> existentClasses;
        private List<System.Drawing.Color> usedColors;
        private bool normalize;
        private int numberOfIterations;
        private int currentIteration;

        private Functions function;
        private double epsilon;
        private double currentEpsilon;

        private double CalculateNormOfVectors(List<double> vector1, List<double> vector2)
        {
            double value = 0;
            for(int i=0; i<vector1.Count; i++)
                value += Math.Pow((vector1[i] - vector2[i]), 2);
            value = Math.Sqrt(value);
            return value;
        }

        private void NormalizeInputPattern(List<double> pattern)
        {
            // Что-то тут не то! Проверить!
            double nn = 0;
            for (int i = 0; i < inputLayerDimension; i++)
            {
                nn += (pattern[i] * pattern[i]);
            }
            nn = Math.Sqrt(nn);
            for (int i = 0; i < inputLayerDimension; i++)
            {
                pattern[i] /= nn;
            }
        }

        private void StartEpoch(List<double> pattern)
        {
            Neuron Winner = this.FindWinner(pattern);
            currentEpsilon = 0;
            for (int i = 0; i < outputLayerDimension; i++)
                for (int j = 0; j < outputLayerDimension; j++)
                {
                    currentEpsilon += outputLayer[i, j].ModifyWights(pattern, Winner.Coordinate, currentIteration, function);                   
                }
            currentIteration++;
            currentEpsilon = Math.Abs(currentEpsilon / (outputLayerDimension * outputLayerDimension));
            EndEpochEventArgs e = new EndEpochEventArgs();
            OnEndEpochEvent(e);
        }

        public bool Normalize
        {
            get { return normalize; }
            set { normalize = value; }
        }

        public List<List<double>> Patterns
        {
            get { return patterns; }
        }

        public List<string> Classes
        {
            get { return classes; }
        }

        public int InputLayerDimension
        {
            get { return inputLayerDimension; }
        }

        public int OutputLayerDimension
        {
            get { return outputLayerDimension; }
        }

        public double CurrentDelta
        {
            get { return currentEpsilon; }
        }

        public SortedList<string, int> ExistentClasses
        {
            get { return existentClasses; }
        }

        public List<System.Drawing.Color> UsedColors
        {
            get { return usedColors; }
        }

        private int NumberOfClasses()
        {
            existentClasses = new SortedList<string, int>();
            existentClasses.Add(classes[0], 1);
            int k = 0;
            int d = 2;
            for (int i = 1; i < classes.Count; i++)
            {
                k=0;
                for (int j = 0; j < existentClasses.Count; j++)
                    if (existentClasses.IndexOfKey(classes[i])!=-1) k++;
                if (k == 0)
                {
                    existentClasses.Add(classes[i],d);
                    d++;
                }
            }
            return existentClasses.Count;
        }

        public System.Drawing.Color[,] ColorSOFM()
        {
            colorMatrix = new Color[outputLayerDimension, outputLayerDimension];
            int numOfClasses = NumberOfClasses();
            List<System.Drawing.Color> goodColors = new List<System.Drawing.Color>();

            goodColors.Add(System.Drawing.Color.Red); //china
            goodColors.Add(System.Drawing.Color.Navy); //france
            goodColors.Add(System.Drawing.Color.Yellow); //germany
            goodColors.Add(System.Drawing.Color.Green); //iran
            goodColors.Add(System.Drawing.Color.Turquoise); //italy
            goodColors.Add(System.Drawing.Color.Black); //other
            goodColors.Add(System.Drawing.Color.DeepSkyBlue); //other european
            goodColors.Add(System.Drawing.Color.Pink); //korea
            goodColors.Add(System.Drawing.Color.Maroon); //spain
            goodColors.Add(System.Drawing.Color.PeachPuff); //uae 
            goodColors.Add(System.Drawing.Color.DarkGray); //hello2
            usedColors = new List<System.Drawing.Color>(numOfClasses);
            for (int i = 0; i < numOfClasses; i++)
            {
                usedColors.Add(goodColors[i]);
            }
            for (int i = 0; i < outputLayerDimension; i++)
                for (int j = 0; j < outputLayerDimension; j++)
                    colorMatrix[i, j] = System.Drawing.Color.FromKnownColor(System.Drawing.KnownColor.ButtonFace);

            for (int i = 0; i < patterns.Count; i++)
            {
               Neuron n = FindWinner(patterns[i]);
                colorMatrix[n.Coordinate.X,n.Coordinate.Y] = usedColors[existentClasses[classes[i]]-1];
            }
            return colorMatrix;
        }

        public NeuralNetwork(int m, int numberOfIterations, double epsilon, Functions f)
        {
            outputLayerDimension = m;
            currentIteration = 1;
            this.numberOfIterations = numberOfIterations;
            function = f;
            this.epsilon = epsilon;
            currentEpsilon = 100;
        }

        public Neuron FindWinner(List<double> pattern)
        {
            List<double> norms = new List<double>(outputLayerDimension * outputLayerDimension);
            double D = 0;
            Neuron Winner = outputLayer[0, 0];
            double min = CalculateNormOfVectors(pattern, outputLayer[0, 0].Weights);
            for (int i = 0; i < outputLayerDimension; i++)
                for (int j = 0; j < outputLayerDimension; j++)
                {
                    D = CalculateNormOfVectors(pattern, outputLayer[i, j].Weights);
                    if (D < min)
                    {
                        min = D;
                        Winner = outputLayer[i, j];
                    }
                }
            return Winner;
        }

        public void StartLearning()
        {
            int iterations = 0;
            while (iterations<=numberOfIterations && currentEpsilon > epsilon)
            {
                List<List<double>> patternsToLearn = new List<List<double>>(numberOfPatterns);
                foreach (List<double> pArray in patterns)
                    patternsToLearn.Add(pArray);
                Random randomPattern = new Random();
                List<double> pattern = new List<double>(inputLayerDimension);
                for (int i = 0; i < numberOfPatterns; i++)
                {
                    pattern = patternsToLearn[randomPattern.Next(numberOfPatterns - i)];

                    StartEpoch(pattern);

                    patternsToLearn.Remove(pattern);
                }
                iterations++;
                OnEndIterationEvent(new EventArgs());
            }
        }

        public void ReadDataFromFile(string inputDataFileName)
        {
            char split = ',';
            StreamReader sr = new StreamReader(inputDataFileName);
            string line = sr.ReadLine();
            int k = 0;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == split) k++;
            }
  
            inputLayerDimension = k;
            int sigma0 = outputLayerDimension;
            
            outputLayer = new Neuron[outputLayerDimension, outputLayerDimension];
            Random r = new Random();
            for (int i = 0; i < outputLayerDimension; i++)
                for (int j = 0; j < outputLayerDimension; j++)
                {
                    outputLayer[i, j] = new Neuron(i, j, sigma0);
                    outputLayer[i, j].Weights = new List<double>(inputLayerDimension);
                    for (k = 0; k < inputLayerDimension; k++)
                    {
                        outputLayer[i, j].Weights.Add(r.NextDouble());
                    }
                }

            k = 0;
            while (line != null)
            {
                line = sr.ReadLine();
                k++;
            }
            patterns = new List<List<double>>(k);
            classes = new List<string>(k);
            numberOfPatterns = k;

            List<double> pattern;

            sr = new StreamReader(inputDataFileName);
            line = sr.ReadLine();
           
            while (line != null)
            {
                int startPos = 0;
                int endPos = 0;
                int j = 0;
                pattern = new List<double>(inputLayerDimension);
                for (int ind = 0; ind < line.Length; ind++)
                {
                    if (line[ind] == split && j != inputLayerDimension)
                    {
                        endPos = ind;
                        pattern.Add(Convert.ToDouble(line.Substring(startPos, endPos - startPos)));
                        startPos = ind + 1;
                        j++;
                    }
                    if (j > inputLayerDimension) throw new InvalidDataException("Wrong file format. Check input data file, and try again");
                }
                if (normalize) this.NormalizeInputPattern(pattern);
                patterns.Add(pattern);
                startPos = line.LastIndexOf(split);
                classes.Add(line.Substring(startPos));
                line = sr.ReadLine();
            }
        }

        public event EndEpochEventHandler EndEpochEvent;
        public event EndIterationEventHandler EndIterationEvent;

        protected virtual void OnEndEpochEvent(EndEpochEventArgs e)
        {
            if (EndEpochEvent != null)
                EndEpochEvent(this, e);
        }

        protected virtual void OnEndIterationEvent(EventArgs e)
        {
            if (EndIterationEvent != null)
                EndIterationEvent(this, e);
        }

        public Color[,] getColorMatrix()
        {
            return colorMatrix;
        }

        public void saveMap(String path)
        {
            int numOfClasses = NumberOfClasses();
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine(inputLayerDimension);
                sw.WriteLine(outputLayerDimension);
                sw.WriteLine(classes.Count);            // number of datarows
                sw.WriteLine(existentClasses.Count);    // number of existent classes
                sw.WriteLine(patterns[0].Count);        // number of columns in patterns

                for (int i = 0; i < classes.Count; i++)  // saving all classes
                {
                    sw.WriteLine(classes[i]);
                }

                foreach (var item in existentClasses)    // saving the list of existent classes (dict)
                {
                    sw.WriteLine(item.Key);
                    sw.WriteLine(item.Value);
                }

                for (int i = 0; i < patterns.Count; i++)    // saving patterns (nested list)
                {
                    for (int j = 0; j < patterns[i].Count; j++)
                    {
                        sw.WriteLine(patterns[i][j]);
                    }
                }

                for (int i = 0; i < outputLayerDimension; i++)  // saving output layer
                {
                    for (int j = 0; j < outputLayerDimension; j++)
                    {
                        for (int k = 0; k < inputLayerDimension; k++)
                        {
                            sw.WriteLine(outputLayer[i, j].Weights[k]);
                        }
                    }
                }

                for (int i = 0; i < outputLayerDimension; i++)  // saving color matrix
                {
                    for (int j = 0; j < outputLayerDimension; j++)
                    {
                        sw.WriteLine(colorMatrix[i, j].Name);
                    }
                }

                for (int i = 0; i < numOfClasses; i++)             // saving usedColors
                {
                    sw.WriteLine(usedColors[i].Name);
                }
            }
        }

        public void loadMap(String path)
        {
            int rows = 0, existentSize = 0, patternCol = 0;

            using (StreamReader sr = new StreamReader(path))
            {
                inputLayerDimension = Convert.ToInt32(sr.ReadLine());
                outputLayerDimension = Convert.ToInt32(sr.ReadLine());

                rows = Convert.ToInt32(sr.ReadLine());
                existentSize = Convert.ToInt32(sr.ReadLine());
                patternCol = Convert.ToInt32(sr.ReadLine());

                classes = new List<string>(rows);
                for (int i = 0; i < rows; i++)
                {
                    classes.Add(sr.ReadLine());
                }

                existentClasses = new SortedList<string, int>();
                for (int i = 0; i < existentSize; i++)
                {
                    existentClasses.Add(sr.ReadLine(), Convert.ToInt32(sr.ReadLine()));
                }

                patterns = new List<List<double>>(rows);
                for (int i = 0; i < rows; i++)
                {
                    List<double> pattern = new List<double>();
                    for (int j = 0; j < patternCol; j++)
                    {
                        pattern.Add(Convert.ToDouble(sr.ReadLine()));
                    }
                    patterns.Add(pattern);
                }

                outputLayer = new Neuron[outputLayerDimension, outputLayerDimension];
                for (int i = 0; i < outputLayerDimension; i++)
                {
                    for (int j = 0; j < outputLayerDimension; j++)
                    {
                        outputLayer[i, j] = new Neuron(i, j, outputLayerDimension);
                        outputLayer[i, j].Weights = new List<double>(inputLayerDimension);
                        for (int k = 0; k < inputLayerDimension; k++)
                        {
                            outputLayer[i, j].Weights.Add(Convert.ToDouble(sr.ReadLine()));
                        }
                    }
                }

                colorMatrix = new Color[outputLayerDimension, outputLayerDimension];
                for (int i = 0; i < outputLayerDimension; i++)
                {
                    for (int j = 0; j < outputLayerDimension; j++)
                    {
                        colorMatrix[i, j] = Color.FromName(sr.ReadLine());
                    }
                }

                int numOfClasses = NumberOfClasses();
                usedColors = new List<Color>();
                for (int i = 0; i < numOfClasses; i++)
                {
                    usedColors.Add(Color.FromName(sr.ReadLine()));
                }
            }
        }
    }
}