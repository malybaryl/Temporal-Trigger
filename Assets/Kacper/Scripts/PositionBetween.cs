using UnityEngine;

public class PositionBetween : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private Transform targetA;
    [SerializeField] private Transform targetB;

    void Update()
    {
        // Zabezpieczenie, ¿eby nie sypa³o b³êdami jak nie przypiszesz obiektów
        if (targetA == null || targetB == null) return;

        // Matematyka: Dodajemy pozycje i dzielimy przez 2
        transform.position = (targetA.position + targetB.position) / 2f;
        
        // Opcjonalnie: Jeœli chcesz, ¿eby te¿ siê obraca³ "pomiêdzy" nimi:
        // transform.rotation = Quaternion.Slerp(targetA.rotation, targetB.rotation, 0.5f);
    }
}