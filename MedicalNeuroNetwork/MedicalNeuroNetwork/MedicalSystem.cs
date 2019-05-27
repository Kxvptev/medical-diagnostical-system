using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MedicalNeuroNetwork
{
    [Serializable]
    public class MedicalSystem
    {
        public string name;
        
        public int age;

        public List<string> diagnosis = new List<string>();  // история болезни, в которой записываются поставленные диагнозы и актуальные на тот момент дата и время

        [NonSerialized]
        public double[] parameters = new double[7];

        public MedicalSystem()
        {

        }

        public static void callNobody()                                                   // что касается методов вызова специалистов
        {                                                                                 // при интеграции данной системы с биосенсорами и контроллерами, методы будут дополнены  
            Console.WriteLine("Больной в норме, помощь специалистов не требуется");       // активацией действий различных устройств (например: инъектор инсулина для диабетиков)
        }                                                                                 // естественно с учётом параметров состояния больного и его историей болезни

        public static void callNurse()
        {
            Console.WriteLine("У больного приступ, вызвана медсестра");
        }

        public static void callDoctor()
        {
            Console.WriteLine("У больного критическое состояние, вызван врач");
        }

        public void temperatureCondDet(double t)
        {
            parameters[0] = 0.0;

            parameters[0] += Math.Abs(t - 36.6) * 0.25;          // состоянию 0 сопоставлена температура 36,6, состоянию 1 - >= 40,6 и <= 32,6

            if (parameters[0] > 1.0)
                parameters[0] = 1.0;
        }

        public void pressureCondDet(int p)
        {
            parameters[1] = 0.0;

            parameters[1] += Math.Abs(p - 120) * 0.017;          // состоянию 0 сопоставлено давление 120, состоянию 1 - >= 180 и <= 60

            if (parameters[1] > 1.0)
                parameters[1] = 1.0;
        }

        public void decision(HashSet<KeyValuePair<string,double[]>> deseases)    // этот метод всё-таки следует разрабатывать совместно с экспертом)
        {
            int criticalCond = 0;          // счётчик параметров, имеющих критическое значение
            int semiCritCond = 0;          // счётчик параметров, имеююхих полукритическое значение
            int complCond = 0;             // счётчик параметров, имеююхих значение говорящее об осложнении
            int semiComplCond = 0;         // счётчик параметров, имеююхих значение говорящее об почти наступившем осложнении

            for (int i = 0; i < 7; i++)
            {
                if (parameters[i] >= 0.95)
                    criticalCond++;
                else if (parameters[i] >= 0.7)
                    semiCritCond++;
                else if (parameters[i] >= 0.5)
                    complCond++;
                else if (parameters[i] >= 0.3)
                    semiComplCond++;
            }

            int totalCond = semiComplCond + complCond * 2 + semiCritCond * 5 + criticalCond * 10; //итоговое состояние рассчитывается как сумма количеств вышеперечисленных параметров
                                                                                                  //с некоторыми коэффицентами. С помощью этой переменной определяется тяжесть состояния больного

            HashSet<string> repet = new HashSet<string>();         //нужен лишь для того, чтобы подсказки о возможном диагнозе не дублировались

            foreach (KeyValuePair<string,double[]> desease in deseases)
            {
                bool coincidence = true;
                for (int i = 0; i < 7; i++)
                    if (Math.Abs(desease.Value[i] - parameters[i]) > 0.2)
                    {
                        coincidence = false;
                        break;
                    }
                if (coincidence && !repet.Contains(desease.Key))
                {
                    repet.Add(desease.Key);
                    Console.WriteLine("Возможно у пациента: {0}", desease.Key);
                }

            }

            if (totalCond < 3)
                callNobody();
            if (totalCond >= 3 && totalCond < 10)
                callNurse();
            if (totalCond >= 10)
                callDoctor();

            Console.WriteLine("Каков диагноз пациента?");           //отвечает эксперт(врач)
            string d = Console.ReadLine();

            double[] temp = new double[7];    //создаём копию массива параметров текущего пациента её для записи в базу данных заболеваний
            Array.Copy(parameters, temp, 7);
            deseases.Add(new KeyValuePair<string, double[]>(d, temp));

            diagnosis.Add(d + ", " + DateTime.Now.ToString("dd MMMM yyyy | HH:mm:ss"));  //делаем запись в историю болезни
        }
    }
}
