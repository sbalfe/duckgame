using Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    private CinemachineVirtualCamera m_MainCamera;

    public CinemachineVirtualCamera GetCurrentCamera()
    {
        return m_MainCamera;
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Game3")
            AttachCamera();
    }

    private void AttachCamera()
    {
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
