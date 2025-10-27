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
        private double estimatedRent;
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
        public void test(double tempNumOfRooms, double tempDistanceToCenter, double tempAge, double tempRent)
        {
            double numOfRooms = tempNumOfRooms / 5;
            double distanceToCenter = tempDistanceToCenter / 20;
            double age = tempAge / 30;

            double v = (numOfRooms * numOfRoomsWeight) + (distanceToCenter * distanceToCenterWeight);
            double sigmoidFunc = 1 / (1 + Math.Pow(Math.E, -v));

            calcMSE(tempRent, sigmoidFunc * 10000);

            estimatedRent = sigmoidFunc * 10000;
        }

        public void calcMSE(double realRent, double expectedRent)
        {
            MSE += Math.Pow(((realRent / 10000) - (expectedRent / 10000)), 2);
            dataNum += 1;
        }

        public string getEstimatedRent()
        {
            return estimatedRent.ToString("F2");
        }
        public double getMSE()
        {
            return MSE / dataNum;
        }
        public double getLearningRate()
        {
            return learningRate;
        }
        public override string ToString()
        {
            return "Estimated Rent: "+ getEstimatedRent()+" MSE: "+getMSE();
        }
    }

    

    private static void Main(string[] args)
    {
        static List<double> readCSV(string path)
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
            return values;
        }
        static artificialNeuron trainingXTimes(artificialNeuron reference_neuron, int epoch)
        {
            artificialNeuron neuron = reference_neuron;
            List<double> trainValues = readCSV("Training_Set.csv");

            for (int n = 0; n < epoch; n++)
            {
                for (int i = 0; i < trainValues.Count; i += 4)
                {
                    neuron.train(trainValues.ElementAt(i), trainValues.ElementAt(i + 1), trainValues.ElementAt(i + 2), trainValues.ElementAt(i + 3));
                }
            }
            return neuron;


        }   
        artificialNeuron neuron1 = trainingXTimes(new artificialNeuron(0.01),25);
        artificialNeuron neuron2 = trainingXTimes(new artificialNeuron(neuron1, 0.05),25);
        artificialNeuron neuron3 = trainingXTimes(new artificialNeuron(neuron1, 0.1),25);

        artificialNeuron neuron4 = trainingXTimes(new artificialNeuron(0.01), 100);
        artificialNeuron neuron5 = trainingXTimes(new artificialNeuron(neuron1, 0.05), 100);
        artificialNeuron neuron6 = trainingXTimes(new artificialNeuron(neuron1, 0.1), 100);
        List<(artificialNeuron neuron, string epoch)> neurons = new List<(artificialNeuron, string)> {
            (neuron1,"25"),
            (neuron2,"25"),
            (neuron3,"25"),
            (neuron4,"100"),
            (neuron5,"100"),
            (neuron6,"100")
        };


        List<double> testValues = readCSV("Test_Set.csv");


        for (int i = 0; i < testValues.Count; i += 4)
        {
            foreach(var n in neurons)
            {
                n.neuron.test(testValues.ElementAt(i), testValues.ElementAt(i + 1), testValues.ElementAt(i + 2), testValues.ElementAt(i + 3));
            }    
        }

        Console.WriteLine("Epoch: ".PadRight(10)+"Learning Rate:".PadRight(15)+"Estimated Rent:".PadRight(20)+"MSE: ".PadRight(15));
        foreach (var n in neurons)
        {    
            Console.WriteLine(n.epoch.PadRight(10)+
                n.neuron.getLearningRate().ToString().PadRight(15)+
                n.neuron.getEstimatedRent().PadRight(20)+
                n.neuron.getMSE().ToString("F5").PadRight(15));
        }
        
    }
}