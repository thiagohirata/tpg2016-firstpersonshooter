using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerInput : NetworkBehaviour
{
    private CharacterController characterController;
    private Transform head;
    private Camera playerViewCamera;
    public float speed = 3.0F;
    public float mouseSensibility = 3.0F;

    public TipoDeTiro tiro;

    // Use this for initialization
    void Start () {
        characterController = GetComponent<CharacterController>();
        head = transform.Find("Head");
        playerViewCamera = head.GetComponent<Camera>();

        if(isLocalPlayer)
        {
            head.GetComponent<Camera>().enabled = true;
            head.GetComponent<GUILayer>().enabled = true;
            head.GetComponent<FlareLayer>().enabled = true;
            head.GetComponent<AudioListener>().enabled = true;
        }
    }



    // Update is called once per frame
    void Update () {
        if(!isLocalPlayer)
        {
            return;
        }


        //movimento do personagem 
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        float forwardSpeed = speed * Input.GetAxis("Vertical");
        Vector3 right = transform.TransformDirection(Vector3.right);
        float rightSpeed = speed * Input.GetAxis("Horizontal");
        characterController.SimpleMove(forward * forwardSpeed + right * rightSpeed);

        //rotação do personagem
        transform.Rotate(0, Input.GetAxis("Mouse X") * mouseSensibility, 0);

        //controle vertical da câmera
        head.Rotate(-1 * Input.GetAxis("Mouse Y") * mouseSensibility, 0, 0);

        //atirar
        if (Input.GetButtonDown("Fire1"))
        {
            //faz um raycast para encontrar o que o tiro vai atingir (como
            //se o raio partisse do centro da tela
            Ray ray = playerViewCamera.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));

            RaycastHit hitInfo;
            if(Physics.Raycast(ray, out hitInfo))
            {
                //hitInfo contem informação sobre onde o tiro atingiu

                //vamos colocar o hiteffect na posição onde o tiro atingiu!
                Quaternion rotation = Quaternion.FromToRotation(Vector3.back, hitInfo.normal);
                CmdSpawnHitEffect(hitInfo.point, rotation);
            }

            StartCoroutine(RecoilCorountine(tiro.recoil));
        }
    }

    [Command]
    void CmdSpawnHitEffect(Vector3 position, Quaternion rotation)
    {
        
        GameObject hitEffect = (GameObject)Instantiate(tiro.hitEffect, position, rotation);
        NetworkServer.Spawn(hitEffect);
    }

    IEnumerator RecoilCorountine(float intensity)
    {
        float recoilTime = 0;
        float totalRecoilTime = 0.08f;

        Vector2 recoilVector = new Vector2(Random.Range(-1 * intensity, intensity), Random.Range(-1 * intensity, intensity));

        while (recoilTime < totalRecoilTime) { 
            recoilTime += Time.deltaTime;
            Vector2 currentRotation = Vector2.Lerp(recoilVector, Vector2.zero, recoilTime / totalRecoilTime);
            this.transform.Rotate(0, currentRotation.x, 0);
            head.Rotate(currentRotation.y, 0, 0);

            yield return new WaitForEndOfFrame();
        }
        
    }


}
