using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MedicalNeuroNetwork
{
    public class Program
    {
        public static int patientCount = 0;                            // последний (текущий) ID
        public static List<string> patientNames = new List<string>();  // список ФИО всех пациентов, прошедших через эту систему

        [Serializable]
        public struct IDPNAndDDB
        {
            public int pCCopy;                                        // копии статических полей, необходимые для их сериализации и последующего восстановления после
            public List<string> pNCopy;                               // повторного запуска программы
            public HashSet<KeyValuePair<string, double[]>> deseases;  // содержит значения параметров, характерных для определённых болезней (исходя из поставленного врачом диагноза)
        }                                                             
     
        static void patientDeserialization(int i)                     // десериализация xml документа, содержащего информацию о конкретном пациенте
        {
            MedicalSystem patientRestored;
            using (StreamReader sr = new StreamReader("patient" + i.ToString() + "DWH.xml"))
            {
                XmlSerializer xs = new XmlSerializer(typeof(MedicalSystem));
                patientRestored = (MedicalSystem)xs.Deserialize(sr);
            }
            Console.WriteLine("Пациент c ID {0}:", i);
            Console.WriteLine("ФИО: {0}", patientRestored.name);
            Console.WriteLine("Возраст(в годах): {0}", patientRestored.age);
            foreach (string d in patientRestored.diagnosis)
                Console.WriteLine(d);
        }

        static void Main(string[] args)
        {
            IDPNAndDDB info = new IDPNAndDDB();
            info.deseases = new HashSet<KeyValuePair<string, double[]>>();

            if (File.Exists("system.dat"))                            // десериализаци и восстановление текущего ID, списка ФИО пациентов и базы данных болезней
            {
                IDPNAndDDB des;
                using (FileStream sr = new FileStream("system.dat", FileMode.Open))
                {
                    BinaryFormatter xs = new BinaryFormatter();
                    des = (IDPNAndDDB)xs.Deserialize(sr);
                }
                patientCount = des.pCCopy;
                patientNames = des.pNCopy;
                info.deseases = des.deseases;
            }

            while (true)
            {
                Console.WriteLine("Для мониторинга пациентов нажмите С, для обращения к базе данных D");
                ConsoleKey key = Console.ReadKey().Key;
                Console.WriteLine();
                if (key == ConsoleKey.C)
                {
                    MedicalSystem currentPatient = new MedicalSystem();

                    Console.WriteLine("Введите ФИО пациента:");
                    currentPatient.name = Console.ReadLine();
                    patientNames.Add(currentPatient.name);

                    Console.WriteLine("Введите возраст пациента:");
                    currentPatient.age = Convert.ToInt32(Console.ReadLine());

                    while (true)
                    {
                        // в идеале подобная система должна работать с биосенсорами, но, по причине отсутсвия у меня таковых, работает с помощью рукописного ввода параметров
                        Console.WriteLine("Введите температуру тела пациента:");
                        currentPatient.temperatureCondDet(Convert.ToDouble(Console.ReadLine()));

                        Console.WriteLine("Введите систолическое артериальное давление пациента (мм. рт. ст.)");
                        currentPatient.pressureCondDet(Convert.ToInt32(Console.ReadLine()));

                        Console.WriteLine("Оцените кашель пациента от 0 до 1 (0 - кашель отсутсвует, 1 - сильный кашель и боль в горле)");
                        currentPatient.parameters[2] = Convert.ToDouble(Console.ReadLine());

                        Console.WriteLine("Оцените степень рвоты пациента от 0 до 1 (0 - чувство тошноты отсутствует, 1 - сильная непрерывная рвота)");
                        currentPatient.parameters[3] = Convert.ToDouble(Console.ReadLine());

                        Console.WriteLine("Оцените степень головной боли пациента от 0 до 1 (0 - боль отсутствует, ясное состояние ума, 1 - сильная головная боль, бред)");
                        currentPatient.parameters[4] = Convert.ToDouble(Console.ReadLine());

                        Console.WriteLine("Оцените состояние лимфоузлов от 0 до 1 (0 - без изменений, 1 - лимфоузлы болят и сильно увеличены)");
                        currentPatient.parameters[5] = Convert.ToDouble(Console.ReadLine());

                        Console.WriteLine("Оцените оттенок кожи пациента от 0 до 1 (0 - без изменений, 1 - бледный, с просвечивающимися венами)");
                        currentPatient.parameters[6] = Convert.ToDouble(Console.ReadLine());

                        currentPatient.decision(info.deseases);

                        Console.WriteLine("Если работа с пациентом закончена, нажмите Q. Иначе любую клавишу");
                        ConsoleKey k = Console.ReadKey().Key;
                        Console.WriteLine();
                        if (k == ConsoleKey.Q)
                        {
                            Console.WriteLine("Контроль пациента {0} (возраст {1} (в годах)) закончен", currentPatient.name, currentPatient.age);
                            using (StreamWriter fs = new StreamWriter("patient" + patientCount.ToString() + "DWH.xml"))    // xml сериализация данных о пациенте после оканчания работы с ним
                            {
                                XmlSerializer xs = new XmlSerializer(typeof(MedicalSystem));
                                xs.Serialize(fs, currentPatient);
                                patientCount++;
                                break;
                            }
                        }
                    }
                }

                else if (key == ConsoleKey.D)
                {
                    if (patientCount != 0)
                    {
                        Console.WriteLine("Для вывода всей информации по пациентам нажмите A, для вывода информации по конкретному пациенту нажмите C");
                        ConsoleKey k = Console.ReadKey().Key;
                        Console.WriteLine();
                        if (k == ConsoleKey.A)
                        {
                            for (int i = 0; i < patientCount; i++)
                            {
                                patientDeserialization(i);
                            }
                        }

                        else if (k == ConsoleKey.C)
                        {
                            Console.WriteLine("Введите ФИО запрашиваемого пациента: ");
                            string request = Console.ReadLine();

                            int id;
                            for (id = 0; id < patientCount; id++)
                                if (patientNames[id] == request)
                                     patientDeserialization(id);       // так как в бд вполне могут оказаться разные пациенты с одинаковым ФИО, цикл при нахождении соответствия не прерывается
                        }
                    }
                    else
                        Console.WriteLine("База данных пуста!");
                }

                Console.WriteLine("Для завершения работы системы нажмите Q. Чтобы приступить к следующему пациенту нажмите любую другую клавишу");
                key = Console.ReadKey().Key;
                if (key == ConsoleKey.Q)
                    break;
            }

            info.pCCopy = patientCount;
            info.pNCopy = patientNames;
            using (FileStream fs = new FileStream("system.dat", FileMode.Create))  // сериализация текущего ID, списка ФИО пациентов и базы данных болезней
            {
                BinaryFormatter xs = new BinaryFormatter();
                xs.Serialize(fs, info);
            }
        }
    }
}
