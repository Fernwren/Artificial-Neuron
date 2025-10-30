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
        private double ageWeight;
        private double learningRate;

        private double MSE;
        private int dataNum;//a variable to count how many times that neuron gets tested
        public artificialNeuron(double learningRate)
        {
            this.learningRate = learningRate;
            numOfRoomsWeight = random.NextDouble() * 2 - 1;//some basic mathematical calculations to expand the random range. this is necessary since this function cannot generate negative values
            distanceToCenterWeight = random.NextDouble() * 2 - 1;

            MSE = 0;
            dataNum = 0;
        }

        public artificialNeuron(artificialNeuron original)//a copy constructer
        {
            numOfRoomsWeight = original.numOfRoomsWeight;
            distanceToCenterWeight = original.distanceToCenterWeight;
            ageWeight = original.ageWeight;
            MSE = original.MSE;
            learningRate = original.learningRate;
        }

        public void train(double tempNumOfRooms, double tempDistanceToCenter, double tempAge, double tempRent)
        {
            //degrading all variables between 0 and 1
            double numOfRooms = tempNumOfRooms / 5; 
            double distanceToCenter = tempDistanceToCenter / 20;
            double age = tempAge / 30;
            double rent = tempRent / 10000;

            double v = (numOfRooms * numOfRoomsWeight) + (age * ageWeight) + (distanceToCenter * distanceToCenterWeight);
            double sigmoidFunc = 1 / (1 + Math.Pow(Math.E, -v));//calculating sigmoid function

            //rearranging weights according to sigmoid function's value
            numOfRoomsWeight += learningRate * (rent - sigmoidFunc) * numOfRooms;
            distanceToCenterWeight += learningRate * (rent - sigmoidFunc) * distanceToCenter;
            ageWeight += learningRate * (rent - sigmoidFunc) * age;
        }

        public double test(double tempNumOfRooms, double tempDistanceToCenter, double tempAge, double tempRent)
        {
            //degrading all variables between 0 and 1
            double numOfRooms = tempNumOfRooms / 5;
            double distanceToCenter = tempDistanceToCenter / 20;
            double age = tempAge / 30;

            double v = (numOfRooms * numOfRoomsWeight) + (age * ageWeight) + (distanceToCenter * distanceToCenterWeight);
            double sigmoidFunc = 1 / (1 + Math.Pow(Math.E, -v));//this time sigmoid function gives us an expected rent value

            calcMSE(tempRent, sigmoidFunc * 10000);//this is a cumulative function to calculate MSE

            return sigmoidFunc * 10000;//since we degraded rent variable by 1/10000, we should multiply it by 10000 when returning it 
        }

        public void calcMSE(double realRent, double expectedRent)//we call this function every time we test the neuron and add up the values
        {
            MSE += Math.Pow((realRent / 10000) - (expectedRent / 10000), 2);
            dataNum += 1;//we increase the dataNum everytime we call calcMSE
        }

        public void setLearningRate(double learningRate)//a function to change a neuron's learning rate manually
        {
            this.learningRate= learningRate;
        }
        public double getMSE()//a function to get current MSE of this neuron
        {
            return MSE / dataNum;//this part is important to get the real MSE. it is written in the formula itself.
        }
        public double getLearningRate()
        {
            return learningRate;
        }
    }
    static List<double> readCSV(string path)//a function to read csv files
    {

        List<double> values = new List<double>();
        string[] lines = File.ReadAllLines(path);
        for (int i = 0; i < lines.Length; i++)
        {
            foreach (string s in lines[i].Split(','))
            {
                values.Add(Double.Parse(s, CultureInfo.InvariantCulture));
            }
        }
        return values;//it returns a single line of data. [a,b,c,d,e,......,n]
    }
    static void trainXTimes(ref artificialNeuron[] referenceNeurons, int epoch, List<double> trainData)//a function to train bunch of neurons specified times with specitied training data
    { 
        for (int n = 0; n < epoch; n++)
        {
            for (int i = 0; i < trainData.Count; i += 4)
            {
                foreach(artificialNeuron neuron in referenceNeurons)
                {
                    neuron.train(trainData.ElementAt(i), trainData.ElementAt(i + 1), trainData.ElementAt(i + 2), trainData.ElementAt(i + 3));
                }
            }
        }
    }

    static void neuronTest(ref artificialNeuron referenceNeuron, List<double> testData)//a function to test a neuron and print the results
    {
        for (int i = 0; i < testData.Count; i += 4)
        {
            Console.WriteLine("{0:0}",referenceNeuron.test(testData.ElementAt(i), testData.ElementAt(i + 1), testData.ElementAt(i + 2), testData.ElementAt(i + 3)));
        }
    }

    private static void Main(string[] args)
    {
        //25 epoch - (trainValues)test neurons
        artificialNeuron neuron1 = new artificialNeuron(0.01);
        artificialNeuron neuron2 = new artificialNeuron(neuron1);neuron2.setLearningRate(0.05);
        artificialNeuron neuron3 = new artificialNeuron(neuron1);neuron3.setLearningRate(0.1);
        //25 epoch - (testValues)test neurons
        artificialNeuron neuron4 = new artificialNeuron(neuron1);
        artificialNeuron neuron5 = new artificialNeuron(neuron2);
        artificialNeuron neuron6 = new artificialNeuron(neuron3);
        //100 epoch - (trainValues)test neurons
        artificialNeuron neuron7 = new artificialNeuron(neuron1);
        artificialNeuron neuron8 = new artificialNeuron(neuron2);
        artificialNeuron neuron9 = new artificialNeuron(neuron3);
        //100 epoch - (testValues)test neurons
        artificialNeuron neuron10 = new artificialNeuron(neuron1);
        artificialNeuron neuron11 = new artificialNeuron(neuron2);
        artificialNeuron neuron12 = new artificialNeuron(neuron3);

        List<double> testValues = readCSV("Test_Set.csv");
        List<double> trainValues = readCSV("Training_Set.csv");

        artificialNeuron[] arr = {neuron1,neuron2,neuron3};
        trainXTimes(ref arr, 25, trainValues);
        
        artificialNeuron[] arr2 = {neuron4,neuron5,neuron6};
        trainXTimes(ref arr2, 25, trainValues);
        
        artificialNeuron[] arr3 = {neuron7,neuron8,neuron9};
        trainXTimes(ref arr3, 100, trainValues);

        artificialNeuron[] arr4 = {neuron10,neuron11,neuron12};
        trainXTimes(ref arr4, 100, trainValues);

        Console.WriteLine("Estimated rents for training data (25 epoch/0.01 learning rate)");
        neuronTest(ref neuron1, trainValues);
        Console.Write("MSE: ");
        Console.WriteLine(neuron1.getMSE().ToString("F5")+ "\n");

        Console.WriteLine("Estimated rents for training data (25 epoch/0.05 learning rate)");
        neuronTest(ref neuron2, trainValues);
        Console.Write("MSE: ");
        Console.WriteLine(neuron2.getMSE().ToString("F5") + "\n");

        Console.WriteLine("Estimated rents for training data (25 epoch/0.1 learning rate)");
        neuronTest(ref neuron3, trainValues);
        Console.Write("MSE: ");
        Console.WriteLine(neuron3.getMSE().ToString("F5") + "\n");

        Console.WriteLine("Estimated rents for test data (25 epoch/0.01 learning rate)");
        neuronTest(ref neuron4, testValues);
        Console.Write("MSE: ");
        Console.WriteLine(neuron4.getMSE().ToString("F5") + "\n");

        Console.WriteLine("Estimated rents for test data (25 epoch/0.05 learning rate)");
        neuronTest(ref neuron5, testValues);
        Console.Write("MSE: ");
        Console.WriteLine(neuron5.getMSE().ToString("F5") + "\n");

        Console.WriteLine("Estimated rents for test data (25 epoch/0.1 learning rate)");
        neuronTest(ref neuron6, testValues);
        Console.Write("MSE: ");
        Console.WriteLine(neuron6.getMSE().ToString("F5") + "\n");

        Console.WriteLine("Estimated rents for train data (100 epoch/0.01 learning rate)");
        neuronTest(ref neuron7, trainValues);
        Console.Write("MSE: ");
        Console.WriteLine(neuron7.getMSE().ToString("F5") + "\n");

        Console.WriteLine("Estimated rents for train data (100 epoch/0.05 learning rate)");
        neuronTest(ref neuron8, trainValues);
        Console.Write("MSE: ");
        Console.WriteLine(neuron8.getMSE().ToString("F5") + "\n");

        Console.WriteLine("Estimated rents for train data (100 epoch/0.1 learning rate)");
        neuronTest(ref neuron9, trainValues);
        Console.Write("MSE: ");
        Console.WriteLine(neuron9.getMSE().ToString("F5") + "\n");

        Console.WriteLine("Estimated rents for test data (100 epoch/0.01 learning rate)");
        neuronTest(ref neuron10, testValues);
        Console.Write("MSE: ");
        Console.WriteLine(neuron10.getMSE().ToString("F5") + "\n");

        Console.WriteLine("Estimated rents for test data (100 epoch/0.05 learning rate)");
        neuronTest(ref neuron11, testValues);
        Console.Write("MSE: ");
        Console.WriteLine(neuron11.getMSE().ToString("F5") + "\n");

        Console.WriteLine("Estimated rents for test data (100 epoch/0.1 learning rate)");
        neuronTest(ref neuron12, testValues);
        Console.Write("MSE: ");
        Console.WriteLine(neuron12.getMSE().ToString("F5") + "\n");
    }
}