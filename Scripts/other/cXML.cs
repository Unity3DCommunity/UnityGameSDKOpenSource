// Creado por Hopesend
// Cerberus Software
// email: cerberussoftware@gmail.com
// 17/02/22011


using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

/// <summary>
/// Clase para la interaccion con XML
/// </summary>
public class cXML
{
    private string raiz;
    /// <summary>
    /// Raiz del XML
    /// </summary>
    public string Raiz { get; set; }

    private Encoding codificacion = Encoding.UTF8;
    /// <summary>
    /// Encoding del XML
    /// </summary>
    public Encoding Codificacion { get; set; }

    private string fichero;
    /// <summary>
    /// Rutal del XML
    /// </summary>
    public string Fichero { get; set; }

    private XmlDocument doc = null;
    private XmlElement root = null;

    #region CONSTRUCTORES
    /// <summary>
    /// Inicializa una nueva Instancia a la clase <see cref="cXML"/>.
    /// </summary>
    public cXML()
    {
        raiz = "oscuridad";
        codificacion = Encoding.UTF8;
    }

    /// <summary>
    /// Inicializa una nueva Instancia a la clase <see cref="cXML"/>.
    /// </summary>
    /// <param name="Raiz">Nombre del raiz del XML</param>
    public cXML(string Raiz)
    {
        raiz = Raiz;
        codificacion = Encoding.UTF8;
    }

    /// <summary>
    /// Inicializa una nueva Instancia a la clase <see cref="cXML"/>.
    /// </summary>
    /// <param name="Raiz">Nombre del raiz del XMK</param>
    /// <param name="Codificacion">Codificacion del XML</param>
    public cXML(string Raiz, Encoding Codificacion)
    {
        raiz = Raiz;
        codificacion = Codificacion;
    }
    #endregion

    #region METODOS

    /// <summary>
    /// Crea el XML
    /// </summary>
    /// <param name="Fichero">path del archivo</param>
    /// <param name="Sobreescribir">bool de operacion interna</param>
    /// <returns>true si lo a creado, false si a fallado</returns>
    public bool Crear(string Fichero, bool Sobreescribir)
    {
        fichero = Fichero;
        return Crear(Sobreescribir);
    }

    /// <summary>
    /// Crea el XML
    /// </summary>
    /// <param name="Sobreescribir">bool de operacion interna</param>
    /// <returns>true si lo a creado, false si a fallado</returns>
    public bool Crear(bool Sobreescribir)
    {
        if (fichero == "") { return false; }
        if (File.Exists(fichero) && !Sobreescribir) { return false; }

        try
        {
            XmlTextWriter f = new XmlTextWriter(fichero, codificacion);
            f.Formatting = Formatting.Indented;
            f.WriteStartDocument();
            f.WriteStartElement(raiz);
            f.WriteEndElement();
            f.Close();
        }
        catch
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Abre un fichero existente en un stream
    /// </summary>
    /// <param name="streamAbrir">el stream donde esta alojado el xml</param>
    /// <returns>true si lo a abierto, false si a fallado</returns>
    public bool Abrir(Stream streamAbrir)
    {
        doc = new XmlDocument();

        try
        {
            doc.Load(streamAbrir);
            root = doc.DocumentElement;
        }
        catch
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Abre un archivo existente
    /// </summary>
    /// <param name="fichero">path del archivo</param>
    /// <returns>true si lo a abierto, false si a fallado</returns>
    public bool Abrir(string fichero)
    {
        this.fichero = fichero;
        if (!File.Exists(fichero)) { return false; }
        doc = new XmlDocument();

        try
        {
            doc.Load(fichero);
            root = doc.DocumentElement;
            raiz = root.Name;
        }
        catch
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Abre un fichero
    /// </summary>
    /// <returns>true si lo a abierto, false si a fallado</returns>
    public bool Abrir()
    {
        if (fichero == "") { return false; }
        if (!File.Exists(fichero)) { return false; }
        doc = new XmlDocument();

        try
        {
            doc.Load(fichero);
            root = doc.DocumentElement;
        }
        catch
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Cierra la instancia abierta
    /// </summary>
    /// <returns>true si lo a cerrado, false sino</returns>
    public bool Cerrar()
    {
        root = null;
        doc = null;
        return true;
    }

    /// <summary>
    /// Guarda el Xml
    /// </summary>
    /// <returns>true si lo a guardado, false si a fallado</returns>
    public bool Grabar()
    {
        try
        {
            doc.Save(this.fichero);
        }
        catch
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Crea un elemento en el XML
    /// </summary>
    /// <param name="Path">rama donde crear el elemento</param>
    /// <param name="Atributos">atributos del elemento</param>
    /// <returns>el nodo creado</returns>
    public XmlNode CrearElemento(string Path, string[] Atributos)
    {
        XmlNode Nodo = root.SelectSingleNode(Path);
        if (Nodo == null)
        {
            try
            {
                XmlElement elemento = doc.CreateElement(Path);
                foreach (string atributo in Atributos)
                {
                    XmlAttribute attr = doc.CreateAttribute(atributo.Split(':')[0]);
                    attr.Value = atributo.Split(':')[1];
                    elemento.Attributes.Append(attr);
                }
                Nodo = root.AppendChild(elemento);
            }
            catch
            {
                return null;
            }
        }

        return Nodo;
    }

    /// <summary>
    /// Crea un elemento
    /// </summary>
    /// <param name="Path">rama donde crear el elemento</param>
    /// <param name="Elemento">nombre del elemento</param>
    /// <param name="Valor">valor del elemento</param>
    /// <returns>el elemento creado</returns>
    public XmlNode CrearElemento(string Path, string Elemento, string Valor)
    {
        XmlNode Nodo = root.SelectSingleNode(Path);
        if (Nodo == null)
        {
            return null;
        }

        try
        {
            XmlElement elemento = doc.CreateElement(Path + "/" + Elemento);
            elemento.InnerText = Valor;
            Nodo.AppendChild(elemento);
        }
        catch
        {
            return null;
        }
        return Nodo;
    }

    /// <summary>
    /// Crea un elemento
    /// </summary>
    /// <param name="Path">rama donde crear el elemento</param>
    /// <param name="Elemento">nombre del elemento</param>
    /// <param name="Atributos">atributos del elemento</param>
    /// <returns>el elemento creado</returns>
    public XmlNode CrearElemento(string Path, string Elemento, string[] Atributos)
    {
        XmlNode Nodo = root.SelectSingleNode(Path);
        if (Nodo == null) { return null; }

        try
        {
            XmlElement elemento = doc.CreateElement(Elemento);
            foreach (string atributo in Atributos)
            {
                XmlAttribute attr = doc.CreateAttribute(atributo.Split(':')[0]);
                attr.Value = atributo.Split(':')[1];
                elemento.Attributes.Append(attr);
            }
            Nodo.AppendChild(elemento);
        }
        catch
        {
            return null;
        }
        return Nodo;
    }

    /// <summary>
    /// Crea un elemento
    /// </summary>
    /// <param name="Path">rama donde crear el elemento</param>
    /// <param name="Elemento">indice del elemento</param>
    /// <param name="Valor">valor del elemento</param>
    /// <param name="Atributos">atributos del elemento</param>
    /// <returns>el elemento creado</returns>
    public XmlNode CrearElemento(string Path, int Elemento, string Valor, string[] Atributos)
    {
        return CrearElemento(Path, Elemento.ToString(), Valor, Atributos);
    }

    /// <summary>
    /// Crear un elemento
    /// </summary>
    /// <param name="Path">rama donde crear el elemento</param>
    /// <param name="Elemento">nombre del elemento</param>
    /// <param name="Valor">valor del elemento</param>
    /// <param name="Atributos">atributos del elemento</param>
    /// <returns>el elemento creado</returns>
    public XmlNode CrearElemento(string Path, string Elemento, string Valor, string[] Atributos)
    {
        XmlNode Nodo = CrearElemento(Path, Elemento, Valor);
        if (Nodo == null)
        {
            return null;
        }
        Nodo = DevolverElementos(Path + "//" + Elemento)[0];

        try
        {
            foreach (string atributo in Atributos)
            {
                XmlAttribute attr = doc.CreateAttribute(atributo.Split(':')[0]);
                attr.Value = atributo.Split(':')[1];
                Nodo.Attributes.Append(attr);
            }
        }
        catch
        {
            return null;
        }

        return Nodo;
    }

    /// <summary>
    /// Inserta un valor a un elemento
    /// </summary>
    /// <param name="ruta">rama donde esta alojado el elemento</param>
    /// <param name="elemento">nombre del elemento</param>
    /// <param name="valor">nuevo valor</param>
    /// <returns>true si a insertado el valor, false sino</returns>
    public bool SetValor(string ruta, string elemento, string valor)
    {
        ruta = ruta + "/" + elemento;
        Console.WriteLine(ruta);
        XmlNode nodo = null;
        try
        {
            nodo = root.SelectSingleNode(ruta);
        }
        catch
        {
        }
        if (nodo == null) return false;
        nodo.InnerText = valor;
        return true;
    }

    /// <summary>
    /// Inserta un valor a un elemento
    /// </summary>
    /// <param name="ruta">rama donde esta alojado el elemento</param>
    /// <param name="elemento">indice del elemento</param>
    /// <param name="valor">nuevo valor</param>
    /// <returns>true si a insertado el valor, false sino</returns>
    public bool SetValor(string Path, int Elemento, string Valor)
    {
        return SetValor(Path, Elemento.ToString(), Valor);
    }

    /// <summary>
    /// Inserta un atributo a un elemento
    /// </summary>
    /// <param name="path">rama del elemento</param>
    /// <param name="elemento">indice del elemento</param>
    /// <param name="atributo">nombre del atributo</param>
    /// <param name="valor">valor del atributo</param>
    /// <returns></returns>
    public bool SetAtributo(string path, int elemento, string atributo, string valor)
    {
        return SetAtributo(path, elemento.ToString(), atributo, valor);
    }

    /// <summary>
    /// Inserta un atributo a un elemento
    /// </summary>
    /// <param name="path">rama del elemento</param>
    /// <param name="elemento">nombre del elemento</param>
    /// <param name="atributo">nombre del nuevo atributo</param>
    /// <param name="valor">valor del nuevo atributo</param>
    /// <returns>true si a insertado el atributo, false sino</returns>
    public bool SetAtributo(string path, string elemento, string atributo, string valor)
    {
        XmlNode Nodo = DevolverElementos(path + "//" + elemento)[0];
        if (Nodo == null)
        {
            return false;
        }

        XmlAttribute attr = Nodo.Attributes[atributo];
        if (attr == null)
        {
            attr = doc.CreateAttribute(atributo);
            attr.Value = valor;
            Nodo.Attributes.Append(attr);
        }
        else
        {
            attr.Value = valor;
        }
        return true;
    }

    /// <summary>
    /// Obtiene la lista de elementos dentro de una rama
    /// </summary>
    /// <param name="path">nombre de la rama</param>
    /// <returns>lista de elementos de la rama</returns>
    public XmlNodeList DevolverElementos(string path)
    {
        return root.SelectNodes(path);
    }

    /// <summary>
    /// Devuelve el valor de un elemento dado
    /// </summary>
    /// <param name="Path">ruta del elemento</param>
    /// <returns>valor del elemento</returns>
    public string DevolverValorElemento(string Path)
    {
        string result = "";

        try
        {
            XmlNode elemento = root.SelectSingleNode(Path);
            result = elemento.InnerText;
        }
        catch { }

        return result;
    }

    /// <summary>
    /// Elimina todos los elementos que coinciden con la ruta
    /// </summary>
    /// <param name="Path">ruta a eliminar</param>
    /// <returns>numero de elementos eliminados</returns>
    public int EliminarElementos(string Path)
    {
        int cont = 0;
        XmlNodeList elementos = root.SelectNodes(Path);
        foreach (XmlNode elemento in elementos)
            try
            {
                elemento.ParentNode.RemoveChild(elemento);
                cont += 1;
            }
            catch
            {
            }

        return cont;
    }

    /// <summary>
    /// Guarda una clase Serializada
    /// </summary>
    /// <typeparam name="T">tipo de Objeto</typeparam>
    /// <param name="ruta">ruta donde guardar el XML</param>
    /// <param name="objetoClase">objeto que contiene los datos</param>
    public void Guardar_Clase_Serializable<T>(string ruta, T objetoClase)
    {
        XmlSerializer objetoSerializado = new XmlSerializer(typeof(T));

        // Creamos un nuevo FileStream para serializar el objeto en un fichero
        TextWriter GrabarFileStream = new StreamWriter(ruta, false);
        objetoSerializado.Serialize(GrabarFileStream, objetoClase);

        GrabarFileStream.Close();
    }

    /// <summary>
    /// Carga una clase Serializada
    /// </summary>
    /// <typeparam name="T">tipo de Objeto</typeparam>
    /// <param name="ruta">ruta del XML</param>
    /// <param name="objetoClase">objeto que contiene los datos</param>
    /// <returns>valor del tipo dado</returns>
    public T Cargar_Clase_Serializable<T>(string ruta, T objetoClase)
    {
        XmlSerializer objetoSerializado = new XmlSerializer(typeof(T));

        // creamos un nuevo FileStream para leer el archivo XML
        FileStream CargarFileStream = new FileStream(ruta, FileMode.Open, FileAccess.Read, FileShare.Read);

        // Cargamos el objeto cargado usando la funcion Deserialize
        T objetoCargado = (T)objetoSerializado.Deserialize(CargarFileStream);

        CargarFileStream.Close();

        return objetoCargado;
    }
    #endregion
}