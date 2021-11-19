using UnityEngine;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    [Range(0.1f, 10f)]
    public float showDamageInterval = 0.2f;
    [Range(0.1f, 10f)]
    public float showExpInterval = 3f;

    private Transform m_FirstPlayer = null;
    private Queue<Transform> m_Players = new Queue<Transform>();

    private int m_PlayerNum = 0;
    private bool m_StartShowDamageText = false;
    private bool m_HideDamageText = false;
    private float m_LastShowDamageTime = 0.0f;
    private float m_LastShowExpTime = 0.0f;
    private Camera m_MainCamera = null;

    private void Start()
    {
        m_MainCamera = Camera.main;
    }

    void AddPlayer()
    {
        ++m_PlayerNum;
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = "Player" + m_PlayerNum.ToString();
        if (m_FirstPlayer == null)
        {
            m_FirstPlayer = obj.transform;
            obj.transform.position = Vector3.zero;
        }
        else
        {
            obj.transform.position = RandomPos();
        }
        obj.transform.localScale = new Vector3(0.25f, 1.0f, 0.25f);
        m_Players.Enqueue(obj.transform);
    }
    void DelAllPlayer()
    {
        foreach(var p in m_Players)
        {
            Destroy(p.gameObject);
        }
        m_Players.Clear();
        m_FirstPlayer = null;
    }

    void DelPlayer()
    {
        if(m_Players.Count <= 0)
        {
            return;
        }
        Transform p = m_Players.Dequeue();
        Destroy(p.gameObject);
        if(m_Players.Count == 0)
        {
            m_FirstPlayer = null;
        }
    }

    Vector3 RandomPos()
    {
        Vector3 vPos = Vector3.zero;

        vPos.x = Random.Range(-5f, 5f);
        vPos.z = Random.Range(-5f, 5f);

        return vPos;
    }

	void Update ()
    {
        if (m_StartShowDamageText)
        {
            float now = Time.time;
            if (m_LastShowDamageTime + showDamageInterval < now)
            {
                m_LastShowDamageTime = now;
                ShowDamageText();
            }
            if(m_FirstPlayer != null && m_LastShowExpTime + showExpInterval < now)
            {
                m_LastShowExpTime = now;
                ShowExpText();
            }
        }

        if(m_MainCamera != null && m_FirstPlayer != null)
        {
            CameraRotate();
            CameraZoom();
        }
    }
    void ShowDamageText()
    {
        foreach (var tf in m_Players)
        {
            int content = Random.Range(10000, 50000);
            if (tf == m_FirstPlayer)
            {
                DamageTextManager.Instance.ShowDamageText("hit", content.ToString(), tf);
            }
            else
            {
                DamageTextManager.Instance.ShowDamageText("attack", content.ToString(), tf);
            }
        }
    }

    void ShowExpText()
    {
        string exp = string.Format("@+{0}", Random.Range(1000, 5000));
        DamageTextManager.Instance.ShowDamageText("exp", exp, m_FirstPlayer);
    }

    private void CameraRotate()
    {
        m_MainCamera.transform.RotateAround(m_FirstPlayer.position, Vector3.up, 20f * Time.deltaTime);
    }

    private void CameraZoom()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            m_MainCamera.transform.Translate(Vector3.forward * 0.5f);
        }


        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            m_MainCamera.transform.Translate(Vector3.forward * -0.5f);
        }
    }

    void OnGUI()
    {
        float left = 100.0f;
        float top, tempTop;
        top = tempTop = 100.0f;
        float gap = 50f;
        float buttonWidth = 150f;
        float buttonHeight = 40f;

        GUI.Box(new Rect(left - 10f, top - 10f, buttonWidth + 20f, 400), "");

        if (GUI.Button(new Rect(left, tempTop, buttonWidth, buttonHeight), "ADD PLAYER"))
        {
            AddPlayer();
        }

        tempTop += gap;
        if (GUI.Button(new Rect(left, tempTop, buttonWidth, buttonHeight), "DEL PLAYER"))
        {
            DelPlayer();
        }

        tempTop += gap;
        if (GUI.Button(new Rect(left, tempTop, buttonWidth, buttonHeight), "ADD PLAYER(20)"))
        {
            for (int i = 0; i < 20; ++i)
            {
                AddPlayer();
            }
        }

        tempTop += gap;
        if (GUI.Button(new Rect(left, tempTop, buttonWidth, buttonHeight), "DEL ALL PLAYER"))
        {
            DelAllPlayer();
        }

        tempTop += gap;
        if (GUI.Button(new Rect(left, tempTop, buttonWidth, buttonHeight), m_StartShowDamageText ? "STOP" : "START"))
        {
            m_StartShowDamageText = !m_StartShowDamageText;
        }

        tempTop += gap;
        if (GUI.Button(new Rect(left, tempTop, buttonWidth, buttonHeight), m_HideDamageText ? "SHOW" : "HIDE"))
        {
            m_HideDamageText = !m_HideDamageText;
            DamageTextManager.Instance.HideAllDamageText(m_HideDamageText);
        }
    }
}
