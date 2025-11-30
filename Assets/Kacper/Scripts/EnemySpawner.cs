using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Konfiguracja Spawnera")]
    [Tooltip("Tablica punktów na mapie (Transformy), gdzie mog¹ pojawiæ siê wrogowie")]
    [SerializeField] private Transform[] spawnPoints;

    [Tooltip("Tablica prefabów wrogów do wylosowania")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Tooltip("Co ile sekund ma siê pojawiaæ nowy wróg")]
    [SerializeField] private float spawnInterval = 5f;

    // Zmienna kontrolna, ¿eby mo¿na by³o zatrzymaæ spawnowanie
    private bool isSpawning = true;

    void Start()
    {
        // Sprawdzenie bezpieczeñstwa - czy tablice nie s¹ puste?
        if (spawnPoints.Length == 0 || enemyPrefabs.Length == 0)
        {
            Debug.LogError("EnemySpawner: Brakuje punktów spawnu lub prefabów wrogów! Uzupe³nij tablice w Inspectorze.");
            return;
        }

        // Uruchomienie pêtli spawnowania
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        // Czekaj 5 sekund (lub mniej) przed pierwszym spawnem - opcjonalne
        // yield return new WaitForSeconds(spawnInterval);

        while (isSpawning)
        {
            SpawnRandomEnemy();
            
            // Czekaj okreœlony czas przed kolejn¹ pêtl¹
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnRandomEnemy()
    {
        // 1. Wylosuj indeks punktu (od 0 do liczby punktów)
        int randomPointIndex = Random.Range(0, spawnPoints.Length);
        Transform selectedPoint = spawnPoints[randomPointIndex];

        // 2. Wylosuj indeks wroga
        int randomEnemyIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject selectedEnemyPrefab = enemyPrefabs[randomEnemyIndex];

        // 3. Stwórz (zinstancjonuj) wroga w wylosowanym miejscu
        Instantiate(selectedEnemyPrefab, selectedPoint.position, selectedPoint.rotation);
    }

    // Opcjonalnie: Rysuje punkty w edytorze, ¿ebyœ widzia³ gdzie s¹
    void OnDrawGizmos()
    {
        if (spawnPoints == null) return;

        Gizmos.color = Color.red;
        foreach (Transform point in spawnPoints)
        {
            if (point != null)
            {
                Gizmos.DrawWireSphere(point.position, 0.5f);
            }
        }
    }
}