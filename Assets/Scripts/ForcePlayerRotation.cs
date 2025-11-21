using UnityEngine;

public class ForcePlayerRotation : MonoBehaviour
{
    [Tooltip("La rotación Y inicial deseada en grados.")]
    public float startYRotation = 50f; // Cambia 0f por la rotación que desees (ej. 180f)

    void Start()
    {
        // Rota el objeto padre del rig (este mismo objeto) al ángulo especificado
        this.transform.rotation = Quaternion.Euler(0, startYRotation, 0);

        // Opcional: Si quieres reiniciar la posición de seguimiento también
        // OVRManager.display.RecenterPose(); 
    }
}
