﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    public enum GameState { Tutorial, Playing, Earthquake};
    public GameState gameState = GameState.Tutorial;
    private bool soundEQPlayed = false;
    private bool soundSPlayed = false;
    private bool preparePoints = false;
    private AudioSource audioPlayer;
    public AudioClip earthquakeSound;
    public AudioClip sirenaSound;
    public AudioClip peopleScreaming;
    public AudioClip womanScreaming;
    public AudioClip peoplePanic;
    public AudioClip AudioPrimerNivel;
    public AudioClip AudioSegundoNivel;
    public AudioClip AudioTercerNivel;
    public AudioClip AudioCuartoNivel;
    public AudioClip pointAgachado;
    public Text CollectablespointsText;
    private int Collectablepoints = 0;
    private int securingPoints = 0;
    public ParticleSystem debris;


    /// <summary>
    /// Objeto que se encarga de "generar" los terremotos del jugador.
    /// </summary>
    private GameObject Terremoto;

    /// <summary>
    /// Cámara principal o en la que se está ejecutando el render, ya 
    /// sea para el casco VR o la de debug.
    /// </summary>
    private Camera MainCamera;

    /// <summary>
    /// Float que representa la altura a en la cuál se verifica si el
    /// jugador se encuentra agachado o no.
    /// </summary>
    public float AlturaAgachado = 2.862f;

    /// <summary>
    /// Valor para almacenar si el jugador se agachó o no durante algúin terremoto.
    /// </summary>
    private bool NoSeAgacho = true;

    /// <summary>
    /// Almacena las veces que un jugador se ha agachado en un terremoto.
    /// el mínimo que puede tener es 0 y el máximo es 3
    /// 0 es no se agacho nunca.
    /// 3 es se agacho en los 3 terremotos.
    /// </summary>
    private int agachado = 0;

    /// <summary>
    /// Valor para almacenar la altura de la cámara de un jugador
    /// </summary>
    private float AlturaCamara;

    /// <summary>
    /// Boolean para asegurar que el metodo de calcular puntos por asegurar objetos se llame una sola vez
    /// </summary>
    private bool pointsSecuringCalculated = false;

    /// <summary>
    /// Lista de objetos asegurables
    /// </summary>
    public GameObject[] securables;

    /// <summary>
    /// Referencia al objeto encargado de ver si el jugador se encuentra o no bajo una mesa.
    /// </summary>
    private GameObject BajoMesa;

    /// <summary>
    /// Almacena las veces que un jugador se ha puesto bajo una mesa en un terremoto.
    /// el mínimo que puede tener es 0 y el máximo es 3
    /// 0 es no se puso bajo una mesa nunca.
    /// 3 es se puso bajo una mesa en los 3 terremotos.
    /// </summary>
    private int mesa = 0;

    /// <summary>
    /// Si el jugador se metió o no debajo de una mesa
    /// en un terremoto
    /// </summary>
    private bool seMetio;

    /// <summary>
    /// Objeto que contiene la referencia a las posiciones
    /// aleatorias. Con el se puede ajustar la posicion aleatoria
    /// de los objetos de primeros auxilios.
    /// </summary>
    private GameObject PosAleatorias;

    //Variable para indicar en que parte del juego estoy
    public int level;
    //Variables para guardar los elementos que deben o no mostrarse 

    private GameObject[] tools;
    private bool showTools;
    private GameObject[] collectables;
    private GameObject[] collectables2;

    private bool showCollectables;
    private float startTime;

    float timeToShake;
    float durationMini;
    float durationBig;

    bool shake1;
    bool shake2;
    bool shake3;

    int cont;

    private GameObject[] unsecuredObjects;

    public Text firstPlaceText;
    public Text secondPlaceText;
    public Text thirdPlaceText;
    public Text yourScoreText;


    //Texto para los niveles
    public Text lvlText;
    public ParticleSystem particulasAgachado;


    public GameObject myButton;


    private void Awake()
    {
        Terremoto = GameObject.Find("Earthquake");
        BajoMesa = GameObject.FindWithTag("bajoMesa");
        audioPlayer = GetComponent<AudioSource>();
        MainCamera = Camera.main;

        PosAleatorias = GameObject.Find("PosicionesAleatorias");
        
        tools = GameObject.FindGameObjectsWithTag("Tool");
        collectables = GameObject.FindGameObjectsWithTag("FirstAid");
        collectables2 = GameObject.FindGameObjectsWithTag("Collectable");

        myButton = GameObject.Find("Button");
    }

    // Use this for initialization
    void Start()
    {
        seMetio = false;
        //nivel inicial 
        level = 0;
        PosAleatorias.GetComponent<PuntosAzar>().DarPosicionesAleatorias();     //Le da una posicion aleatoria a los objetos de primeros auxilios
                                                                                // objetos que deben aparecer o desaparecen
        timeToShake = 30f;
        durationMini = 5f;
        durationBig = 10f;
        shake1 = false;
        shake2 = false;
        shake3 = false;

        cont = 0;
        securables = GameObject.FindGameObjectsWithTag("Securable");
        firstPlaceText.text = "1.   " +GetFirstPlaceScore().ToString();
        secondPlaceText.text = "2.   " +GetSecondPlaceScore().ToString();
        thirdPlaceText.text = "3.   " + GetThirdPlaceScore().ToString();
        startTime = timeToShake;
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (level)
        {
            case 1:
                dropCoverHoldLvl();
                break;
            case 2:
                collectablesLvl();
                break;
            case 3:
                toolsLvl();
                break;
            case 4:
                mainGame();
                break;
        }

    }

    //Cambia el nivel actual
    public void changeLevel()
    {
        level++;
        print(level);
        startTime = Time.time + 5f;
        switch (level)
        {
            case 1:
                lvlText.text = "Nivel 1 \n Esconderse bajo la mesa";
                showElements(false, collectables);
                showElements(false, tools);
                audioPlayer.PlayOneShot(AudioPrimerNivel, 1f);
                startTime = Time.time + 30f;
                break;
            case 2:
                audioPlayer.PlayOneShot(AudioSegundoNivel, 1f);
                collectablesLvl();
                showElements(true, collectables);
                lvlText.text = "Nivel 2 \n Prepara el botiquin";
                startTime = Time.time + 60f;
                break;

            case 3:
                audioPlayer.PlayOneShot(AudioTercerNivel, 1f);
                showElements(false, collectables);
                showElements(true, tools);
                startTime = Time.time + 130f;
                lvlText.text = "Nivel 3 \n Asegura los muebles ";
                

                break;
            case 4:
                pointsSecuringCalculated = false;
                foreach (GameObject securable in securables)
                {
                    securable.SendMessage("Restart");
                }
                audioPlayer.PlayOneShot(AudioCuartoNivel, 1f);
                lvlText.text = "Nivel 4 \n Aplica lo aprendido";
                CollectablespointsText.text = "0/6";
                Collectablepoints = 0;
                showElements(true, collectables);
                showElements(true, collectables2);
                showElements(true, tools);
                PosAleatorias.GetComponent<PuntosAzar>().DarPosicionesAleatorias();
                myButton.GetComponent<Button>().interactable = false;
                startTime = Time.time + 60f;
                break;
        }
    }

    //Detecta si se debe llamar al terremoto
    public bool isShakeTime()
    {
        if (Time.time >= startTime & Time.time <= startTime + durationMini)
        {
            correrTerremoto(true);
            return true;
        }
        return false;

    }

    //Hace aparecer o desaparecer los elementos del arreglo que se pasa por parametro
    public void showElements(bool isShowing, GameObject[] theObjects)
    {
        foreach (GameObject obj in theObjects)
        {
            obj.SetActive(isShowing);
        }
    }

    public void dropCoverHoldLvl()
    {
        isShakeTime();
    }

    public void collectablesLvl()
    {
        isShakeTime();
    }

    public void toolsLvl()
    {
        if (Time.time >= startTime & Time.time <= startTime + durationMini)
        {
            if (!pointsSecuringCalculated)
            {     
                //Calcula los puntos obtenidos por el usuario hasta el momento del terremoto (por asegurar objetos)
                foreach (GameObject securable in securables)
                {
                    securable.SendMessage("EarthquakeResult");
                }
                pointsSecuringCalculated = true;
            }
        }
        isShakeTime();


    }

    /// <summary>
    /// Prepara las variables para el siguiente terremoto
    /// </summary>
    public void limpiar()
    {
        seMetio = false;
        NoSeAgacho = true;

    }

    /// <summary>
    /// Corre todo lo del terremoto y revisa si la persona esta agachada y debajo de una mesa.
    /// </summary>
    /// <param name="aCorrerPequenio">True para correr el terremoto pequeño, false para correr el grande</param>
    public void correrTerremoto(bool aCorrerPequenio)
    {
        if(aCorrerPequenio)
        {
            Terremoto.GetComponent<Earthquake_shake>().CorrerPequeño();
        }
        else
        {
            Terremoto.GetComponent<Earthquake_shake>().CorrerFuerte();       
        }
        

        bool mesa = BajoMesa.GetComponent<DetectaJugador>().EstaBajoMesa();
        if (mesa && !seMetio)
        {
            seMetio = true;
            IncreaseDropCoverPoints("mesa");

        }

        if (NoSeAgacho)
        {
            JugadorAgachado();
        }

        float AlturaCamara = MainCamera.transform.position.y;
        if (AlturaCamara <= AlturaAgachado && NoSeAgacho)
        {
            IncreaseDropCoverPoints("agachado");
            NoSeAgacho = false;
        }
    }

    public void mainGame()
    {
        
        if (!preparePoints){
            securingPoints = 0;
            Collectablepoints = 0;
            agachado = 0;
            mesa = 0;
            preparePoints = true;
        }
        if (cont < 2)
        {
            if (Time.time >= startTime & Time.time <= startTime + durationMini)
            {
                correrTerremoto(true);
            }

            if (Time.time >= startTime + durationMini)
            {
                startTime = Time.time + timeToShake;
                cont++;
                print("entre" + cont);
                limpiar();

            }
        }
        else
        {
            if (Time.time >= startTime & Time.time <= startTime + durationBig)
            {
                if (!pointsSecuringCalculated)
                {
                    //Calcula los puntos obtenidos por el usuario hasta el momento del terremoto (por asegurar objetos)
                    foreach (GameObject securable in securables)
                    {
                        securable.SendMessage("EarthquakeResult");
                    }
                    pointsSecuringCalculated = true;

                    calculateFinalScore();
                }
                correrTerremoto(false);
                if (!soundEQPlayed)
                {
                    audioPlayer.PlayOneShot(earthquakeSound, 1);
                    audioPlayer.PlayOneShot(peopleScreaming, 0.1f);
                    audioPlayer.PlayOneShot(peoplePanic, 0.5f);
                    GameObject baby = GameObject.FindGameObjectWithTag("Baby");
                    baby.SendMessage("Cry");
                    debris.Play();
                    soundEQPlayed = true;
                    GameObject vent = GameObject.FindGameObjectWithTag("vent");
                    vent.GetComponent<Animator>().SetBool("fall",true);
                }
            }

            //AUMENTAR EL TIEMPO QUE NO SE HAGA HASTA QUE TODOS LOS OBJETOS HAYAN TERMINADO DE CAER.
            if (Time.time >= startTime)
            {
                if (!soundSPlayed)
                {
                    audioPlayer.PlayOneShot(sirenaSound, 0.1f);
                    audioPlayer.PlayOneShot(womanScreaming, 0.2f);
                    soundSPlayed = true;
                    unsecuredObjects = GameObject.FindGameObjectsWithTag("Fallen");
                    debris.Stop();
                    foreach (GameObject fallen in unsecuredObjects)
                    {
                        Debug.Log(fallen.name);
                    }
                }
            }

        }       
    }

    public void IncreasePoints()
    {
         CollectablespointsText.text = (++Collectablepoints).ToString() + "/6";

    }
    public void IncreasePoints2()
    {
        CollectablespointsText.text = "Bien";

    }

    public void IncreasePoints3()
    {
        CollectablespointsText.text = "Mal";

    }
    public void IncreasePointsSecuring(float points)
    {
        securingPoints += (int)points;
        Debug.Log("Total Points" + securingPoints);
    }

    public void StartGame()
    {
        gameState = GameState.Playing;
        StartCoroutine("CargarEscena"); //Llama al enumerador para cargar la escena
    }

    /// <summary>
    /// Enumerador encargado de cargar la escena próxima y descargar la
    /// actual de forma asyncrónica.
    /// </summary>
    /// <returns></returns>
    private IEnumerator CargarEscena()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Single);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return 1;
        }
    }


    /// <summary>
    /// Revisa si el jugador se encuentra o no agachado cuando es llamado.
    /// Modifica el atributo NoSeAgacho para reflejar esta información.
    /// </summary>
    private void JugadorAgachado() {
        //Revisa la altura del jugador para determinar si está agachado
        AlturaCamara = MainCamera.transform.position.y;
        if (AlturaCamara <= AlturaAgachado) {
            NoSeAgacho = false;
            particulasAgachado.Play();
            audioPlayer.PlayOneShot(pointAgachado, 1f);
            Debug.Log("Se agacho");
            IncreaseDropCoverPoints("agachado");//incrementa los puntos por agacharse.
        }
    }

    private void IncreaseDropCoverPoints(string accion) {
        switch (accion)
        {
            case "agachado":
                agachado++;
                break;
            case "mesa":
                mesa++;
                break;
            default:
                print("accion no valida");
                break;

        }            
    }

    public void calculateFinalScore()
    {
        int secondPlacePoints = GetSecondPlaceScore();
        int firstPlacePoints = GetFirstPlaceScore();
        int totalPoints = (Collectablepoints * 200) + securingPoints + (agachado * 200) + (mesa * 350);
        yourScoreText.text = "Tus puntos: " + totalPoints.ToString();
        if (totalPoints >= GetThirdPlaceScore())
        {
            if(totalPoints >= secondPlacePoints)
            {
                if (totalPoints >= firstPlacePoints)
                {
                    thirdPlaceText.text = "3.   " + secondPlacePoints.ToString();
                    SaveScoreThirdPlace(secondPlacePoints);
                    secondPlaceText.text = "2.   " + firstPlacePoints.ToString();
                    SaveScoreSecondPlace(firstPlacePoints);
                    firstPlaceText.text = "1.   " + totalPoints.ToString();
                    firstPlaceText.color = Color.green;
                    SaveScoreFirstPlace(totalPoints);
                }
                else
                {
                    thirdPlaceText.text = "3.   " + secondPlacePoints.ToString();          
                    SaveScoreThirdPlace(secondPlacePoints);
                    secondPlaceText.text = "2.   " + totalPoints.ToString();
                    secondPlaceText.color = Color.green;
                    SaveScoreSecondPlace(totalPoints);
                }
            }
            else
            {
                thirdPlaceText.text = "3.   " + totalPoints.ToString();
                thirdPlaceText.color = Color.green;
                SaveScoreThirdPlace(totalPoints);
            }
        }
    }

    public int GetThirdPlaceScore()
    {
        return PlayerPrefs.GetInt("Third Place Points", 0);
    }
    public void SaveScoreThirdPlace(int points)
    {
        PlayerPrefs.SetInt("Third Place Points", points);
    }
    public int GetSecondPlaceScore()
    {
        return PlayerPrefs.GetInt("Second Place Points", 0);
    }
    public void SaveScoreSecondPlace(int points)
    {
        PlayerPrefs.SetInt("Second Place Points", points);
    }
    public int GetFirstPlaceScore()
    {
        return PlayerPrefs.GetInt("First Place Points", 0);
    }
    public void SaveScoreFirstPlace(int points)
    {
        PlayerPrefs.SetInt("First Place Points", points);
    }
}
