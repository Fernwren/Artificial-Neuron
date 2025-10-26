using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Transactions;

internal class Program
{
    public static Random random = new Random();
    class artificialNeuron
    {
        private double numOfRoomsWeight;
        private double distanceToCenterWeight;
        
        private double learningRate;

        private double MSE;
        private int dataNum;
        public artificialNeuron(double learningRate)
        {
            this.learningRate = learningRate;
            numOfRoomsWeight = random.NextDouble() * 2 - 1;
            distanceToCenterWeight = random.NextDouble() * 2 - 1;
            
            MSE = 0;
            dataNum = 0;
        }

        public artificialNeuron(artificialNeuron original, double learningRate)
        {
            this.numOfRoomsWeight = original.numOfRoomsWeight;
            this.distanceToCenterWeight = original.distanceToCenterWeight;
            
            this.MSE = original.MSE;
            this.learningRate = learningRate;
        }

        public void train(double tempNumOfRooms, double tempDistanceToCenter, double tempAge, double tempRent)
        {
            double numOfRooms = tempNumOfRooms / 5;
            double distanceToCenter = tempDistanceToCenter / 20;
            double age = tempAge / 30;
            double rent = tempRent / 10000;

            double v = (numOfRooms * numOfRoomsWeight) + (distanceToCenter * distanceToCenterWeight);
            double sigmoidFunc = 1 / (1 + Math.Pow(Math.E, -v));



            numOfRoomsWeight += learningRate * (rent - sigmoidFunc) * numOfRooms;
            distanceToCenterWeight += learningRate * (rent - sigmoidFunc) * distanceToCenter;

        }
        public double test(double tempNumOfRooms, double tempDistanceToCenter, double tempAge, double tempRent)
        {
            double numOfRooms = tempNumOfRooms / 5;
            double distanceToCenter = tempDistanceToCenter / 20;
            double age = tempAge / 30;

            double v = (numOfRooms * numOfRoomsWeight) + (distanceToCenter * distanceToCenterWeight);
            double sigmoidFunc = 1 / (1 + Math.Pow(Math.E, -v));

            calcMSE(tempRent,sigmoidFunc*10000);

            return sigmoidFunc * 10000;
        }

        public void calcMSE(double realRent, double expectedRent)
        {
            MSE += Math.Pow(((realRent/10000)-(expectedRent/10000)),2);
            dataNum += 1;
        }

        public double getMSE()
        {
            return MSE/dataNum;
        }
    }
    private static void Main(string[] args)
    {
        artificialNeuron neuron1 = new artificialNeuron(0.01);
        artificialNeuron neuron2 = new artificialNeuron(neuron1,0.05);
        artificialNeuron neuron3 = new artificialNeuron(neuron1, 0.1);
        artificialNeuron neuron4 = new artificialNeuron(neuron1, 0.01);
        artificialNeuron neuron5 = new artificialNeuron(neuron1, 0.05);
        artificialNeuron neuron6 = new artificialNeuron(neuron1, 0.1);

        List<double> trainValues = new List<double>();
        string[] lines = File.ReadAllLines("Training_Set.csv");
        for (int i = 0; i < lines.Length; i++)
        {
            foreach (string s in lines[i].Split(','))
            {
                trainValues.Add(Double.Parse(s,CultureInfo.InvariantCulture));
            }
        }

        List<double> testValues = new List<double>();
        string[] lines2 = File.ReadAllLines("Test_Set.csv");
        for (int i = 0; i < lines2.Length; i++)
        {
            foreach (string s in lines2[i].Split(','))
            {
                testValues.Add(Double.Parse(s, CultureInfo.InvariantCulture));
            }
        }

        for (int n = 0; n < 25; n++)
        {
            for (int i = 0; i < trainValues.Count; i += 4)
            {
                neuron1.train(trainValues.ElementAt(i), trainValues.ElementAt(i + 1), trainValues.ElementAt(i + 2), trainValues.ElementAt(i + 3));
                neuron2.train(trainValues.ElementAt(i), trainValues.ElementAt(i + 1), trainValues.ElementAt(i + 2), trainValues.ElementAt(i + 3));
                neuron3.train(trainValues.ElementAt(i), trainValues.ElementAt(i + 1), trainValues.ElementAt(i + 2), trainValues.ElementAt(i + 3));
            }
        }

        for (int n = 0; n < 100; n++)
        {
            for (int i = 0; i < trainValues.Count; i += 4)
            {
                neuron4.train(trainValues.ElementAt(i), trainValues.ElementAt(i + 1), trainValues.ElementAt(i + 2), trainValues.ElementAt(i + 3));
                neuron5.train(trainValues.ElementAt(i), trainValues.ElementAt(i + 1), trainValues.ElementAt(i + 2), trainValues.ElementAt(i + 3));
                neuron6.train(trainValues.ElementAt(i), trainValues.ElementAt(i + 1), trainValues.ElementAt(i + 2), trainValues.ElementAt(i + 3));
            }
        }

        //for (int i = 0; i < trainValues.Count; i += 4)
        {
            //Console.WriteLine(neuron1.test(trainValues.ElementAt(i), trainValues.ElementAt(i + 1), trainValues.ElementAt(i + 2), trainValues.ElementAt(i + 3)));
        }

        for (int i = 0; i < testValues.Count; i += 4)
        {
            Console.WriteLine(neuron1.test(testValues.ElementAt(i), testValues.ElementAt(i + 1), testValues.ElementAt(i + 2), testValues.ElementAt(i + 3)));
            Console.WriteLine(neuron2.test(testValues.ElementAt(i), testValues.ElementAt(i + 1), testValues.ElementAt(i + 2), testValues.ElementAt(i + 3)));
            Console.WriteLine(neuron3.test(testValues.ElementAt(i), testValues.ElementAt(i + 1), testValues.ElementAt(i + 2), testValues.ElementAt(i + 3)));
            Console.WriteLine(neuron4.test(testValues.ElementAt(i), testValues.ElementAt(i + 1), testValues.ElementAt(i + 2), testValues.ElementAt(i + 3)));
            Console.WriteLine(neuron5.test(testValues.ElementAt(i), testValues.ElementAt(i + 1), testValues.ElementAt(i + 2), testValues.ElementAt(i + 3)));
            Console.WriteLine(neuron6.test(testValues.ElementAt(i), testValues.ElementAt(i + 1), testValues.ElementAt(i + 2), testValues.ElementAt(i + 3)));
        }

        Console.WriteLine(neuron1.getMSE());
        Console.WriteLine(neuron2.getMSE());
        Console.WriteLine(neuron3.getMSE());
        Console.WriteLine(neuron4.getMSE());
        Console.WriteLine(neuron5.getMSE());
        Console.WriteLine(neuron6.getMSE());
    }
}