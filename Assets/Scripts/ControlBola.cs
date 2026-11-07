
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ControlBola : MonoBehaviour
{
    public float velocidadMovimiento = 5f;
    public float fuerzaMaxima = 15f;
    public float tiempoCarga = 2f;

    public BoxCollider basePista;

    public Transform fondoFuerza;
    public Transform barraFuerza;

    private Rigidbody cuerpoBola;
    private SphereCollider colisionadorBola;

    private float fuerzaActual = 0f;
    private float movimiento = 0f;

    private float anchoMaximo;
    private float altoBarra;
    private float profundidadBarra;
    private float bordeIzquierdo;
    private float posicionY;
    private float posicionZ;

    private bool cargando = false;
    private bool lanzada = false;
    private bool reiniciando = false;

    void Start()
    {
        cuerpoBola = GetComponent<Rigidbody>();
        colisionadorBola = GetComponent<SphereCollider>();

        cuerpoBola.useGravity = false;
        cuerpoBola.isKinematic = false;

        if (fondoFuerza != null && barraFuerza != null)
        {
            anchoMaximo = fondoFuerza.localScale.x;
            altoBarra = fondoFuerza.localScale.y;
            profundidadBarra = fondoFuerza.localScale.z;

            bordeIzquierdo = fondoFuerza.localPosition.x - anchoMaximo / 2f;
            posicionY = fondoFuerza.localPosition.y;
            posicionZ = barraFuerza.localPosition.z;

            ActualizarBarra(0f);
        }
    }

    void Update()
    {
        if (Keyboard.current == null || reiniciando)
            return;

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            ReiniciarEscena();
            return;
        }

        if (!lanzada)
        {
            MoverBola();
            CargarFuerza();
        }
    }

    void MoverBola()
    {
        movimiento = 0f;

        if (Keyboard.current.leftArrowKey.isPressed ||
            Keyboard.current.aKey.isPressed)
        {
            movimiento = -1f;
        }
        else if (Keyboard.current.rightArrowKey.isPressed ||
                 Keyboard.current.dKey.isPressed)
        {
            movimiento = 1f;
        }
    }


    void FixedUpdate()
    {
        if (cuerpoBola == null || basePista == null || colisionadorBola == null)
            return;

        Bounds limites = basePista.bounds;
        float radioBola = colisionadorBola.bounds.extents.x;

        float limiteIzquierdo = limites.min.x + radioBola;
        float limiteDerecho = limites.max.x - radioBola;

        Vector3 nuevaPosicion = cuerpoBola.position;

        if (!lanzada)
        {
            nuevaPosicion.x += movimiento * velocidadMovimiento * Time.fixedDeltaTime;
        }

        nuevaPosicion.x = Mathf.Clamp(
            nuevaPosicion.x,
            limiteIzquierdo,
            limiteDerecho
        );

        cuerpoBola.MovePosition(nuevaPosicion);
    }


    void CargarFuerza()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && !cargando)
        {
            cargando = true;
            fuerzaActual = 0f;
            ActualizarBarra(0f);
        }

        if (cargando && Keyboard.current.spaceKey.isPressed)
        {
            fuerzaActual += (fuerzaMaxima / tiempoCarga) * Time.deltaTime;
            fuerzaActual = Mathf.Clamp(fuerzaActual, 0f, fuerzaMaxima);

            ActualizarBarra(fuerzaActual / fuerzaMaxima);
        }

        if (cargando && Keyboard.current.spaceKey.wasReleasedThisFrame)
        {
            LanzarBola();
        }
    }

    void ActualizarBarra(float porcentaje)
    {
        if (barraFuerza == null || fondoFuerza == null)
            return;

        porcentaje = Mathf.Clamp01(porcentaje);

        float ancho = anchoMaximo * porcentaje;

        barraFuerza.localScale = new Vector3(
            ancho,
            altoBarra,
            profundidadBarra
        );

        barraFuerza.localPosition = new Vector3(
            bordeIzquierdo + ancho / 2f,
            posicionY,
            posicionZ
        );
    }

    void LanzarBola()
    {
        cargando = false;
        lanzada = true;
        movimiento = 0f;

        cuerpoBola.linearVelocity = Vector3.zero;
        cuerpoBola.angularVelocity = Vector3.zero;

        cuerpoBola.AddForce(
            Vector3.back * fuerzaActual,
            ForceMode.Impulse
        );
    }

    void LateUpdate()
    {
        if (!lanzada || basePista == null ||
            cuerpoBola == null || reiniciando)
            return;

        Bounds limites = basePista.bounds;
        Vector3 posicion = cuerpoBola.position;

        if (posicion.z < limites.min.z ||
            posicion.z > limites.max.z ||
            posicion.x < limites.min.x ||
            posicion.x > limites.max.x)
        {
            ReiniciarEscena();
        }
    }

    void ReiniciarEscena()
    {
        if (reiniciando)
            return;

        reiniciando = true;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}
