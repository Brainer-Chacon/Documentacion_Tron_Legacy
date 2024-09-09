using System ; 
using System. Collections.Generic; 
using System.Dynamic;
using System.Drawing; 
using System. Windows.Forms;  
using System.Threading.Tasks;
using System.Configuration;

//Clase que representa la moto sus propiedades visuales. 
public class Moto
{
    public delegate void MotoDestruidaEventHandler(Moto moto);
    public Point Posicion  {get; set; }  // Posicion de la moto 
    public Size Tamaño  {get; set;}     // Tamaño de moto 
    public Color ColorChasis {get; set;}      // color chasis 
    public Color  ColorRuedas {get; set;}      // Color Ruedas 

    public int Velocidad {get; private set; }  //Velocidad de moto 
    public int TamañoEstela {get; set; }    //Tamaño de estela de la  moto 
    public int Combustible {get; set; }   //Nivel de Combustible 
    public Queue<string> Items {get; private set; }   //cola de items 
    public Stack<string> Poderes {get; private set; }  // Pila de poderes
    public List<Point> Estela {get; set; }  //Estela que deja la moto tras su paso
    public Direction Direccion {get; set; }   //Direccion en la que se mueve la moto 
    public bool MotoDestruid {get; set; } 
    public event Action<Moto> MotoDestruida; //Evento a lanzar una vez se destruye la moto 
    public Moto (Point posicion, Size tamaño, Color colorChasis, Color colorRuedas) //Constructor de clase 
    {
        this.Posicion = posicion;
        this.Tamaño = tamaño;
        this.ColorChasis = colorChasis;
        this.ColorRuedas = colorRuedas;

        Random rnd = new Random (); 
        this.Velocidad = rnd.Next(1,11) ; //velocidad definida entre 1 y 10 
        this.TamañoEstela = 3; //Tamaño inicial de estela
        this.Combustible = 100; //Combustible inicial maximo 
        this.Items = new Queue<string> (); //Cola e items Vacia 
        this.Poderes = new Stack<string>(); // Pila de poderes vacias 
        this.Estela = new List<Point>();   //Lista ed estela 
        this.Direccion = Direction.Right; //Direccion inicial 
    }
    public void Dibujar (Graphics g)  //Dibujo de moto en pantalla 
    {
        //Dibujar la estela de la moto 
        foreach (var punto in Estela)
        {
            using (Brush brushEstela = new SolidBrush(Color.FromArgb(128, ColorChasis)))  //Color semi-Trsnparente para la estala 
            {
                g.FillRectangle(brushEstela, new Rectangle (punto, Tamaño)); 
            }
        }
        //Dibujar el chasis de la moto como un rectangulo.
        Rectangle rectChasis = new Rectangle(Posicion, Tamaño);  
        using (Brush brushChasis = new SolidBrush(ColorChasis))
        {
            g.FillRectangle(brushChasis, rectChasis);
        }
        //Dibujar Ruedas 
        Size tamañoRueda = new Size(Tamaño.Width / 4, Tamaño.Height / 2);
        Point posicionRuedaDelantera = new Point(Posicion.X, Posicion.Y + Tamaño.Height / 2 - tamañoRueda.Height / 2);
        Point posicionRuedaTrasera = new Point(Posicion.X + Tamaño.Width - tamañoRueda.Width, Posicion.Y + Tamaño.Height / 2 - tamañoRueda.Height / 2);

        using (Brush brushRuedas = new SolidBrush(ColorRuedas)) //Creacion del metodo Brush para dibujo de ruedas
        {
            g.FillEllipse(brushRuedas, new Rectangle(posicionRuedaDelantera, tamañoRueda));
            g.FillEllipse(brushRuedas, new Rectangle(posicionRuedaTrasera, tamañoRueda));
        }
        
    }
    
    //Metodo de Movimiento de la moto y disminucion de combustible 
    public void Mover ()
    {
        this.Posicion = new Point (Posicion.X + Velocidad, Posicion.Y); //Movimiento de la moto hacia la derecha 
        
        Estela.Add(new Point(Posicion.X, Posicion.Y)); 
        if (Estela.Count > TamañoEstela)
        {
            Estela.RemoveAt(0);  //Mantener tamaño de estela
        }

        //Movimiento de la moto basada en la direccion actual 
        switch (Direccion)
        {
            case Direction.Up: 
                Posicion = new Point(Posicion.X, Posicion.Y - Velocidad); 
                break; 
            case Direction.Down: 
                Posicion = new Point(Posicion.X, Posicion.Y + Velocidad); 
                break; 
            case Direction.Left: 
                Posicion = new Point (Posicion.X - Velocidad, Posicion.Y);  
                break; 
            case Direction.Right: 
                Posicion = new Point(Posicion.X + Velocidad, Posicion.Y); 
                break; 
        }

        //Calculo de consumo de combustible 
        int Consumo = Velocidad / 5;   // 1 unidad de combustible por cada 5 unidades de velocidad 
        Combustible = Math.Max (0, Combustible - Consumo) ; 
        if (Combustible == 0 ) //Verifica si el conbustible es 0
        {
            DestruirMoto();   //en caso de ser 0, destruir moto
        }

        if (MotoDestruid)
        {
            OnMotoDestruida(moto); 
        }
    }

    //Metodo que verifica si la moto ha chocado con otra
    public void VerificarColision(Moto moto,Moto otraMoto)
    {
        if (Posicion == otraMoto.Posicion || Estela.Contains(otraMoto.Posicion))
        {
            DestruirMoto(); 
            otraMoto.DestruirMoto(); 
        }
    }

    //Metodo para añadir un item de la cola de items. 
    public void AñadirItem (string item) 
    {
        Items.Enqueue (item); 
    }
    //Metodo para utlizar un item de la cola de items. 
    public string UsarItem()
    {
        return Items.Count > 0 ? Items.Dequeue() : null; 
    }

    //Metodo para añadir un poder a la pila de podedres de la moto
    public void AñadirPoder (string poder) 
    {
        Poderes.Push (poder); 
    }
    //Metodo para utilizar un poder de la pila 
    public string UsarPoder()
    {
        return Poderes.Count > 0 ? Poderes.Pop() : null; 
    } 
    //Metodo que se llama cuando la moto es destruia 
    //Distribuye los items y poderes en el mapa en posiciones random 
    private void DestruirMoto()
    {
        MotoDestruida?.Invoke(this);  //Lanza el evento de destruccion 

        while (Items.Count > 0) //Distribuir items en el mapa 
        {
            Point posicionAleatoria = GenerarPosicionAleatoria (); 
            string item = Items.Dequeue (); 
            ColocarEnMapa (item, posicionAleatoria); 
        }
        while (Poderes.Count > 0)   //Distribuir poderes en el mapa 
        {
            Point posicionAleatoria = GenerarPosicionAleatoria (); 
            string poder = Poderes.Pop(); 
            ColocarEnMapa(poder, posicionAleatoria); 
        }
        MotoDestruid = true; 
    }
    //Metodo que genera una posicion aleatoria en el mapa 
    private Point GenerarPosicionAleatoria()  
    {
        Random rnd = new Random(); 
        int x = rnd.Next (0,800); //Asumiento ancho de mapa
        int y = rnd.Next (0,600);  //Asumiento alto de mapa 
        return new Point (x, y) ; 
    }
    //Metodo que coloca item o poder en una posicion en mapa 
    private void ColocarEnMapa (string nombre, Point posicion)
    {
        Console.WriteLine($"Colocando {nombre} en la posicion ({posicion.X}, {posicion.Y})"); 
    }

    //Intercambia el poder en el tope de la pila con el siguiente 
    public void MoverPoderHaciaAbajo ()
    {
        if (Poderes.Count > 1 )
        {
            //Extraer los dos primeros poderes
            string poder = Poderes.Pop();     //Saca el poder del tope 
            Stack <string> tempStack = new Stack<string>() ; 

            //Invertir la pila hasta el fondo
            while (Poderes.Count > 0)
            {
                tempStack.Push(Poderes.Pop()); 
            }
            //Volver a armar la pila con el poder al fondo
            Poderes.Push(poder); 
            while (tempStack.Count > 0)
            {
                Poderes.Push(tempStack.Pop()); 
            }
        }
    }

    //  Metodo para aplicar los items de la cola automaticamente con delay (1 seg.)
    public async Task AplicarItemsConDelay()
    {
        while (Items.Count > 0)
        {
            string item = Items.Dequeue (); 
            if (item == "Combustible" && Combustible < 100) //Si el items es una celda sde combustible y este no esta lleno
            {
                Combustible = Math.Min (100, Combustible +10); //Aumenta 10 unidades
                Console.WriteLine ($"Se ha aplicao de item: {item}, Combustible actual {Combustible}"); 
            }
            else if (item == "Combustible" && Combustible == 100)
            {
                //Si el combustible esta lleno, se vuele a insertar en la cola sin aplicarse 
                Items.Enqueue(item); 
                Console.WriteLine ("El combustible esta lleno, el item se a reintegrao en la cola."); 
            }
            else 
            {
                Console.WriteLine ($"Se ha applicado el item: {item}."); 
            }
            await Task.Delay(1000); //Esperar 1 segundo antes de aplicar el siguiente item.  
        }
    }

    //Metodo que define las posiciones posibles de movimiento de la moto
    public enum Direction 
    {
        Up, 
        Down, 
        Left, 
        Right
    }
    }

    //Formulario principal del juego donde se muestra la moto y sus atributos 
    public class MotoForm : Form 
    {
        private Moto moto; 
        private Button btnMoverPoder; 
        private Button btnUsarPoder; 
        private ListBox lstPoderes; 
        public System.Windows.Forms.Timer gameTimer {get; set; }  
        

        public MotoForm ()
        {
            this.Text = "Moto Game"; 
            this.Size = new Size (800,600); 
            this.moto = new Moto(new Point(100, 100), new Size(50, 20), Color.Blue, Color.Black);

            
            //moto = new Moto (new Point(100,100), new Size (50,20), Color.Blue, Color.Black); 
            
            //Configuracion de controles 
            btnMoverPoder = new Button {Text = "Mover Poder", Location = new Point(10,10)}; 
            btnMoverPoder.Click += (sender, args) => moto.MoverPoderHaciaAbajo(); 

            btnUsarPoder = new Button {Text = "Usar Poder", Location = new Point(10,50)}; 
            btnUsarPoder.Click += (sender, args) => moto.UsarPoder(); 

            lstPoderes = new ListBox {Location = new Point(10,90), Size = new Size(100,150)}; 

            this.Controls.Add(btnMoverPoder); 
            this.Controls.Add(btnUsarPoder); 
            this.Controls.Add(lstPoderes); 

            //Simular la aplicacion de items con delay
            Task.Run(()=> moto.AplicarItemsConDelay()); 

            //Configurar el timer del juego 
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 100; // Intervalo de movimiento de la moto (100 ms)
            timer.Tick += (sender, e) =>
            {
                moto.Mover(); 
                this.Invalidate(); 
            };
            timer.Start(); 

 

            //manejo de teclas para cambiar la dirreccion de la moto 
            this.KeyDown += new KeyEventHandler(MotoForm_KeyDown);

 
        }
    //Metodo que actualiza el estado del juego en cada timer 
    private void ActualizarJuego(MotoForm moto)
    {
        moto.Mover(); 
        this.Invalidate(); 
    }

    //Metodo que se llama cuando se desteuye la moto 
    private void OnMotoDestruida()
    {
        try 
        {
            gametimer.Stop(); 
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al detener el timer {ex.Message}"); 
        }
        MessageBox.Show ("la moto ha sido destruida!"); 
    }


    //Maneja los eventos de teclas para cambiar la direccion de la moto 
    private void MotoForm_keyDown(object sender, KeyEventArgs e, MotoForm moto) 
    { 
        switch (e KeyCode)
        
            case Keys.Up: 
                moto.Direccion = Direction.Up; 
                break; 
            case Keys.Down: 
                moto.Direccion = Direction.Down;  
                break; 
            case Keys.Left: 
                moto.Direccion = Direction.Left;  
                break; 
            case Keys.Right: 
                moto.Direccion = Direction.Right;  
                break; 
         
    }
    protected override void OnPaint (PaintEventArgs e)
    {
        base.OnPaint (e); 
        moto.Dibujar (e.Graphics); 
    }

}
