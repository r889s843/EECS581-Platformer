using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float speed;
    private float currentPosX;
    private Vector3 velocity = Vector3.zero; 
    [SerializeField] private Transform player;

    private void Update() {
        //transform.position = Vector3.SmoothDamp(transform.position, new Vector3(currentPosX, transform.position.y, transform.position.z, ref velocity, speed * Time.deltaTime));

        //follow player
        transform.position = new Vector3(player.position.x, transform.position.y, transform.position.z);
    }
}
