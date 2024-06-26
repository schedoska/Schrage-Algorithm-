using System;
using System.Collections.Generic;
using GeneratorCS;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        int C_max = 0;
        List<Zadanie> zestaw = GenerateSet(12, 29, 324);
        C_max = Utils.Calculate_times(zestaw);
        Console.WriteLine("Brak zmiany kolejności: Cmax = " + C_max + "\n");

        List<Zadanie> t_shrage = Schrage.schrage(zestaw);
        C_max = Utils.Calculate_times(t_shrage);
        Utils.Display_List(t_shrage);
        Console.WriteLine("Schrage: Cmax = " + C_max + "\n");

        Console.WriteLine("Schrage_pmtn: Cmax = " + Schrage.schrage_pmnt(zestaw) + "\n");
        
        List<Zadanie> t_carlier = Carlier.carlier(zestaw);
        C_max = Utils.Calculate_times(t_carlier);
        Utils.Display_List(t_carlier);
        Console.WriteLine("Carlier: Cmax = " + C_max);
        Console.WriteLine(Utils.IsCorrect(t_carlier));
    }

    static public List<Zadanie> GenerateSet(int size, int range, long seed, int X = -1)
    {
        List<Zadanie> new_set = new List<Zadanie>();
        RandomNumberGenerator generator = new RandomNumberGenerator(seed); 

        int a = 0;
        for(int q = 0; q < size; q++)
        {
            Zadanie zad_temp = new Zadanie(q + 1);
            zad_temp.p_j = generator.nextInt(1, range);
            new_set.Add(zad_temp);
            a += zad_temp.p_j;
        }
        foreach(Zadanie zad_temp in new_set)
        {
            zad_temp.r_j = generator.nextInt(1, a);
        }
        foreach(Zadanie zad_temp in new_set)
        {
            if (X == -1) zad_temp.q_j = generator.nextInt(1, a);
            else zad_temp.q_j = generator.nextInt(1, X);
        }
        return new_set;
    }    
}

public static class Schrage
{
    static public List<Zadanie> schrage(List<Zadanie> zestaw)
    {
        List<Zadanie> Ready_Tasks = new List<Zadanie>();
        List<Zadanie> Not_scheduled_Tasks = Utils.CopyZadanieList(zestaw);
        List<Zadanie> Lista_Kolejnosci = new List<Zadanie>();

        Not_scheduled_Tasks.Sort((x,y) => x.r_j.CompareTo(y.r_j));   // od najmniejszej wart czasu dostepności
        int currentTime = Not_scheduled_Tasks.First().r_j;          // Rozpoczecie w chwili dostępności 1 zad.
        
        while(Ready_Tasks.Count != 0 || Not_scheduled_Tasks.Count != 0)
        {
            while(Not_scheduled_Tasks.Count != 0 && Not_scheduled_Tasks.First().r_j <= currentTime)
            {
                Ready_Tasks.Add(Not_scheduled_Tasks.First());
                Not_scheduled_Tasks.RemoveAt(0);
            }
            if(Ready_Tasks.Count != 0)  //Jesli są gotowe zadanie do realizacji
            {
                Ready_Tasks.Sort((x,y) => x.q_j.CompareTo(y.q_j)); 
                Zadanie temp = Ready_Tasks.Last(); // Z najdłuższym czasem zakończenia
                Lista_Kolejnosci.Add(temp);
                Ready_Tasks.RemoveAt(Ready_Tasks.Count - 1);
                currentTime = currentTime + temp.p_j;
            }
            else
            currentTime = Not_scheduled_Tasks.First().r_j;  //Przejdź do dostępności kolejnego
        }
        return Lista_Kolejnosci;
    }
    static public List<Zadanie> schrage_PriorityQueue(List<Zadanie> zestaw) // Schrage na kolejkach
    {
        PriorityQueue<Zadanie, int> Ready_Tasks = new PriorityQueue<Zadanie, int>();
        PriorityQueue<Zadanie, int> Not_scheduled_Tasks = new PriorityQueue<Zadanie, int>();
        List<Zadanie> Lista_Kolejnosci = new List<Zadanie>();

        foreach(Zadanie zad in zestaw) Not_scheduled_Tasks.Enqueue(zad, zad.r_j);
        int currentTime = Not_scheduled_Tasks.Peek().r_j;

        while(Ready_Tasks.Count != 0 || Not_scheduled_Tasks.Count != 0)
        {
            while(Not_scheduled_Tasks.Count != 0 && Not_scheduled_Tasks.Peek().r_j <= currentTime)
            {
                Zadanie temp = Not_scheduled_Tasks.Dequeue();
                Ready_Tasks.Enqueue(temp, -temp.q_j);   // W kolejności od największego q_j do najmniejszego
            }
            if(Ready_Tasks.Count != 0)
            {
                Zadanie temp = Ready_Tasks.Dequeue();   // o najdłuższym czasie dostarczenia q
                Lista_Kolejnosci.Add(temp);
                currentTime = currentTime + temp.p_j;
            }
            else
            currentTime = Not_scheduled_Tasks.Peek().r_j;
        }
        return Lista_Kolejnosci;
    }
    static public int schrage_pmnt(List<Zadanie> zestaw)    // Schrage z przerwaniami
    {
        List<Zadanie> Ready_Tasks = new List<Zadanie>();
        List<Zadanie> Not_scheduled_Tasks = Utils.CopyZadanieList(zestaw);
        int C_max = 0;
        Zadanie currentZadanie = new Zadanie(0, Int32.MaxValue);

        Not_scheduled_Tasks.Sort((x,y) => x.r_j.CompareTo(y.r_j)); // od najmniejszego czasu dostępności
        int currentTime = Not_scheduled_Tasks.First().r_j;
        
        while(Ready_Tasks.Count != 0 || Not_scheduled_Tasks.Count != 0)
        {
            while(Not_scheduled_Tasks.Count != 0 && Not_scheduled_Tasks.First().r_j <= currentTime)
            {
                Zadanie temp = Not_scheduled_Tasks.First();
                Ready_Tasks.Add(temp);
                Not_scheduled_Tasks.RemoveAt(0);    //dodanie koljnych dostępnych zadań
                if(currentZadanie.q_j < temp.q_j)   //Sprawdzenie czy dosyępny nie ma wiekszego q niż poprzednie zad
                {
                    currentZadanie.p_j = currentTime - temp.r_j;  //obliczenie ile zadania musi dokończyć (może być -)
                    currentTime = temp.r_j; // zmiana czasu do rozpoczęcia koljenego zadania 
                    if(currentZadanie.p_j > 0)      
                    {
                        Ready_Tasks.Add(currentZadanie); //Jeśli zostało do wykonaina do wraca do listy gotowych zadań
                    }
                }
            }
            if(Ready_Tasks.Count != 0)
            {
                Ready_Tasks.Sort((x,y) => x.q_j.CompareTo(y.q_j)); 
                currentZadanie = Ready_Tasks.Last();    // Wymierane jest z najdłuższym czasem dostarczenia q
                Ready_Tasks.RemoveAt(Ready_Tasks.Count - 1);
                currentTime = currentTime + currentZadanie.p_j;  // Przejście do chwili po zakończeniu zadania
                C_max = Math.Max(C_max, currentTime + currentZadanie.q_j);
            }
            else
            currentTime = Not_scheduled_Tasks.First().r_j;
        }
        return C_max;
    }
}

public static class Carlier
{
    static private int UB;
    static private List<Zadanie> Lista_Kolejnosci;

    static Carlier()
    {
        UB = Int32.MaxValue;
        Lista_Kolejnosci = new List<Zadanie>();
    }

    static public List<Zadanie> carlier(List<Zadanie> zestaw)
    {
        List<Zadanie> lista_pi = Schrage.schrage(zestaw);   //Wyznaczenie kolejności zadan algorytmem Schrage
        int U = Utils.Calculate_times(lista_pi);

        if(U < UB){                                         // Jeśli znalezione rozwiązanie jest lepsze niż do tej pory to zaktualizuj
            UB = U;                                         // Górne oszacowanie
            Lista_Kolejnosci = Utils.CopyZadanieList(lista_pi);
        }

        //Blok krytyczny
        int b = Calculate_b(lista_pi, U);                   //ostatnie zadanie ścieżki krytycznej
        int a = Calculate_a(lista_pi, U, b);                //pierwsze zadania ścieżki krytycznej
        int c = Calculate_c(lista_pi, a, b);                //Pierwsze zadanie przed b ale z mniejszym czasem dostarczenia q 
        if(c == -1) return Lista_Kolejnosci;                //Jesli nie istnieje takie zadanie to znaleziono optymalną kolejnosc

        List<Zadanie> K = Create_K(Utils.CopyZadanieList(lista_pi), c, b);  //Zadania od c+1 do b
        K.Sort((x,y) => x.r_j.CompareTo(y.r_j));    
        int _r = K.First().r_j;                             //Najszybszy czas dostępności z K
        K.Sort((x,y) => x.q_j.CompareTo(y.q_j));
        int _q = K.First().q_j;                             //Najkrótszy czas dostarczenia z K
        int _p = 0;
        foreach(Zadanie zad in K) _p += zad.p_j;            //Suma czasów wykonywania w K

        //Wymuszenie realizacji zadania c dopiero po wykoaniu się zadań ze scieżki
        int r_pi_c = lista_pi.ElementAt(c).r_j;
        lista_pi.ElementAt(c).r_j = Math.Max(r_pi_c, _r + _p);
        int LB = Schrage.schrage_pmnt(lista_pi);            // Po wykonaniu zaminay czasy rozpoczęcia sprawdzenie dolnego ograniczenia
        if(LB < UB){                                        //Jesli jest mniejsze czyli potencjalnie lepsze
            carlier(lista_pi);                              //Wykonaj algorytm Carlier ponownie
        }
        lista_pi.ElementAt(c).r_j = r_pi_c;                 //przywróć orginalny stan czasu dostępności

        //Wymuszenie relizacji zadnia c szybcie przez wydłużenie czasu dostarczenia q przed zadaniami ze sciezki
        int q_pi_c = lista_pi.ElementAt(c).q_j;
        lista_pi.ElementAt(c).q_j = Math.Max(q_pi_c, _q + _p);
        LB = Schrage.schrage_pmnt(lista_pi);                // Ewaluacja nowego dolnego ograniczenia
        if(LB < UB){                                         //Jesli jest mniejsze to może być potencjalnie lepsze
            carlier(lista_pi);                              //Wykonaj algorytm Carlier ponownie
        }
        lista_pi.ElementAt(c).q_j = q_pi_c;                 //przywróć orginalny stan czasu dostarczenia

        return Lista_Kolejnosci;
    }

    public static int Calculate_b(List<Zadanie> zestaw, int C_max)
    {
        for(int j = zestaw.Count - 1; j >= 0; j--)
        {
            Zadanie temp = zestaw.ElementAt(j);
            if(temp.C_j + temp.q_j == C_max) return j;
        } 
        return -1;
    }
    public static int Calculate_a(List<Zadanie> zestaw, int C_max, int b)
    {
        int q_b = zestaw.ElementAt(b).q_j;
        for(int j = 0; j < zestaw.Count; j++)
        {
            int Suma_p = 0;
            for(int k = j; k <= b; k++)
            {
                Suma_p += zestaw.ElementAt(k).p_j;
            }
            if(C_max == zestaw.ElementAt(j).r_j + Suma_p + q_b) return j;
        }
        return -1;
    }
    public static int Calculate_c(List<Zadanie> zestaw, int a, int b)
    {
        int q_b = zestaw.ElementAt(b).q_j;
        for(int j = b - 1; j >= a; j--)
        {
            if(zestaw.ElementAt(j).q_j < q_b) return j;
        }
        return -1;
    }
    public static List<Zadanie> Create_K(List<Zadanie> zestaw, int c, int b)
    {
        List<Zadanie> K_list = new List<Zadanie>();
        for(int j = c + 1; j <= b; j++)
        {
            K_list.Add(zestaw.ElementAt(j).Copy());
        }
        return K_list;
    }
}

public class Zadanie
{
    public int Id;
    public int p_j, r_j, S_j = 0, C_j = 0, q_j;
    public Zadanie(int id, int q = 0, int p = 0, int r = 0)
    {
        Id = id;
        q_j = q;
        p_j = p;
        r_j = r;
    }
    public void print() => Console.WriteLine("[ " + Id + " ] Start: " + S_j + " , End: " + C_j
    + " , p_j: " + p_j + " , r_j: " + r_j + " , q_j: " + q_j);
    public Zadanie Copy()
    {
        Zadanie nowe = new Zadanie(Id);
        nowe.p_j = this.p_j;
        nowe.r_j = this.r_j;
        nowe.S_j = this.S_j;
        nowe.C_j = this.C_j;
        nowe.q_j = this.q_j;
        return nowe;
    }
}

class Utils
{
    public static List<Zadanie> CopyZadanieList(List<Zadanie> list)
    {
        List<Zadanie> nowe = new List<Zadanie>();
        foreach(Zadanie zad in list)
        {
            nowe.Add(zad.Copy());
        }
        return nowe;
    }
    static public void Display_List(List<Zadanie> list)
    {
        foreach(Zadanie zad in list)
        {
            zad.print();
        }
    }
    static public int Calculate_times(List<Zadanie> list)
    {
        int C_max = 0;
        int End_of_last = 0;
        foreach(Zadanie zad in list)
        {
            if(zad.r_j > End_of_last)
                zad.S_j = zad.r_j;
            else
                zad.S_j = End_of_last;

            End_of_last = zad.C_j = zad.S_j + zad.p_j;
            C_max = Math.Max(C_max, zad.C_j + zad.q_j);
        }
        return C_max;
    } 
    static public bool IsCorrect(List<Zadanie> list)
    {
        int EndOfLast = 0;
        foreach(Zadanie zad in list)
        {
            if(zad.S_j < zad.r_j) return false;
            if(zad.S_j < EndOfLast) return false;
            if(zad.C_j != zad.S_j + zad.p_j) return false;
            EndOfLast = zad.C_j;
        }
        return true;
    }
}
