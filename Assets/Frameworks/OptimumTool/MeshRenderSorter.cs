using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class MeshRenderSorter : MonoBehaviour
{
	public int sortingOrder;
	MeshRenderer m_MeshRenderer;
	void Awake()
	{
		m_MeshRenderer = GetComponent<MeshRenderer>();
		if (m_MeshRenderer != null)
			m_MeshRenderer.sortingOrder = sortingOrder;
	}
}