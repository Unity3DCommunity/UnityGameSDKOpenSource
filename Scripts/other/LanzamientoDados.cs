// Creado por Hopesend
// Cerberus Software
// email: cerberussoftware@gmail.com
// 17/02/22011

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Reflection;
	
public class LanzamientoDados
{
    ArrayList tiradas = new ArrayList();
	
    public LanzamientoDados()
	{
    }

    public double[] Lanzar()
    {
        if (tiradas.Count == 0) return new double[] {-1};

        tirada[] ts = (tirada[])tiradas.ToArray(typeof(tirada));
        return Lanzar(ts);
    }

    public double Lanzar(tirada Tirada)
    {
        return Tirada.Resolver();
    }

    public double[] Lanzar(tirada[] tiradas)
    {
        double[] result = new double[tiradas.Length];

        for (int i = 0; i <= tiradas.Length-1; i++)
        {
            tirada t = tiradas[i];
            result[i] = t.Resolver();
        }

        return result;
    }

    public int Lanzar(string expresion)
    {
        tirada t = new tirada(expresion);
        return t.Resolver();
    }

    public int[] Lanzar(params string[] expresiones)
    {
        int[] result = new int[expresiones.Length];

        for (int i = 0; i <= expresiones.Length - 1; i++)
        {
            tirada t = new tirada(expresiones[i]);
            result[i] = t.Resolver();
        }

        return result;
    }

    public void Add(tirada Tirada)
    {
        tiradas.Add(Tirada);
    }

    public void Clear()
    {
        tiradas.Clear();
    }
}

public class tirada
{
    protected enum eDados
    {
        D4 = 4,
        D6 = 6,
        D8 = 8,
        D10 = 10,
        D12 = 12,
        D20 = 20,
        D30 = 30,
        D100 = 100
    }

    public string expresion;
    private string[] patterns = { "[\\d]*(dado)\\[[\\d]*(-)[\\d]*\\]", "[\\d]*(dado)" };

	public tirada(string Expresion)
    {
		expresion = Expresion;
    } 

    internal int Resolver()
    {
        //si no viene expresión, devolver -1
        if (expresion == "") return -1;

        //iniciliazar el random
        Random random = new Random();

        //guardar la expresión en una cadena temporal
        string expaux = expresion;
        
        //cambiar los valores de cadena de los dados (D10, D20, D6, etc...) a valores numéricos
        //recorremos los valores del enum al reves, para cambiar antes los D100 que los D10
        int[] valores = (int[]) System.Enum.GetValues(typeof(eDados));
        for (int i = valores.Length - 1; i >= 0; i--)
        {
            //dado a buscar
            string d = "D" + valores[i].ToString();
            foreach (string pattern in patterns) 
            {
                //expresión regular para encontrar los dados necesarios del valor indicado
                string reg = pattern.Replace("dado", d);
                Regex rx = new Regex(reg);
                MatchCollection matches = rx.Matches(expaux);
                foreach (Match match in matches)
                {
                    string exp = match.Value;
                
                    //si el dado encontrado no tiene un número que le preceda, la cantidad de dados a tirar es 1
                    int cantidad = 1;
                    int min = 1;
                    int max = valores[i];
                    if (exp.IndexOf("D") > 0)
                    {
                        cantidad = int.Parse(exp.Split('D')[0]);
                        max = valores[i] * cantidad;
                    }
                    if (exp.IndexOf("[") > 0 && exp.IndexOf("]") > 0)
                    {
                        string limites = exp.Substring(exp.IndexOf("[") + 1, exp.IndexOf("]") - exp.IndexOf("[") - 1);
                        min = int.Parse(limites.Split('-')[0]);
                        max = int.Parse(limites.Split('-')[1]);
                    }

                    //eDados dado = (eDados)valores[i];
                    expaux = expaux.Replace(exp, ValorRandom(random, cantidad, min, max, valores[i]).ToString());
                }
            }
        }

		int result = 0;
		try
		{
			result = Evaluate (expaux); 
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
   			result = 0;
		}

        return result;
	}

	public int Evaluate(string expression)  
	{  
		return (int)new System.Xml.XPath.XPathDocument  
			(new System.IO.StringReader("<r/>")).CreateNavigator().Evaluate  
				(string.Format("number({0})", new  
				               System.Text.RegularExpressions.Regex(@"([\+\-\*])").Replace(expression, " ${1} ").Replace("/", " div ").Replace("%", " mod ")));  
	} 



    private int ValorRandom(Random random, int cantidad, int min, int max, int limite)
    {
        int result;
        
        do {
            result = 0;
            for (int i = 1; i <= cantidad; i++)
                result += random.Next(0, limite) + 1;
        } while (result < min || result > max);

        return result;
    }
}