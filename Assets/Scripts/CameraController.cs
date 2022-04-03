using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    private CinemachineVirtualCamera m_MainCamera;

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Game2")
            AttachCamera();
    }

    private void AttachCamera()
    {
        if (SceneManager.GetActiveScene().name == "Game2")
        {
            AttachCamera();
            m_MainCamera = GameObject.FindObjectOfType<CinemachineVirtualCamera>();
            Assert.IsNotNull(m_MainCamera, "CameraController.AttachCamera: Couldn't find gameplay freelook camera");

            if (m_MainCamera)
            {
                // camera body / aim
                m_MainCamera.Follow = transform;
                m_MainCamera.LookAt = transform;
            }
        }
    }
}
