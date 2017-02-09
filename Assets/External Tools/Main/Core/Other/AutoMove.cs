using UnityEngine;
using System.Collections;


public class  AutoMove : MonoBehaviour 
{
	public Vector3 	targetPoint = Vector3.zero;
	public float 	time 		= 2;  
	public bool 	isPingPong 	= false;
	public bool 	isLocal 	= false;
	private bool 	m_increase 	= true;
	private Vector3	m_posINI;
	private float 	m_rate 		= 0;	
	private float 	m_i 		= 0; 
	private float 	m_timePrev 	= 2; 
	

	void Start () 
	{
		Inicialize();
		m_timePrev = time;
	}


	void Update () 
	{
		if(time <= 0)
			time = 0.01f;
		if(m_timePrev != time){
			m_rate = 1.0f/time;
			m_timePrev = time;
		}
		UpdatePosition();
	}


	void Inicialize()
	{
		if (isLocal) {
			m_posINI = transform.localPosition;
		} else {
			m_posINI = transform.position;
		}
		m_rate = 1.0f/time;
		m_i = 0;
	}

	
	void UpdatePosition()
	{
		if (m_i < 1.0f) { 
			m_i += Time.deltaTime * m_rate;
			if (m_increase) {
				if (isLocal) {
					transform.localPosition = Vector3.Lerp (m_posINI, targetPoint, m_i);
				} else {
					transform.position = Vector3.Lerp (m_posINI, targetPoint, m_i);
				}
			} else {
				if (isLocal) {
					transform.localPosition = Vector3.Lerp (targetPoint, m_posINI, m_i);
				} else {
					transform.position = Vector3.Lerp (targetPoint, m_posINI, m_i);
				}
			}
		} else {
			if (isPingPong) { 
				m_increase = !m_increase;
				m_i = 0;
			}
		}
	}
	
	
}