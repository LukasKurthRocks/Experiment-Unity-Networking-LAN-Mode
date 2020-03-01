using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Just a "basic" object spawner with a little directional thrust.
// Added a gizmo just for showing. Do not need a mesh for this.
// Should work on the removal of cubes sometime when rezising the list.

public class MiniCubeSpawner : MonoBehaviour {
    [Header("Pipe Preferences")]
    [SerializeField]
    private int _maxObject = 250;
    [SerializeField]
    private float _timeGap = .1F;
    [SerializeField]
    private GameObject _miniCubePrefab = null;
    [SerializeField]
    private List<GameObject> _createdCubes = null;
    [SerializeField]
    private bool _canSpawn = true;
    [SerializeField]
    private float thrust = 35F;
    [SerializeField]
    private Vector3 _direction = Vector3.left;

    [Header("Debug Preferences")]
    [SerializeField]
    private bool _overrideGizmo = false;

    void Start() {
        if (_miniCubePrefab == null)
            Debug.LogWarning("PipeController::Start(): _miniCubePrefab is null.");

        StartCoroutine(CubeInstantiator());
    }

    private void OnDrawGizmos() {
        // Only sho gizmo when not selected
        if (UnityEditor.Selection.activeGameObject == gameObject && !_overrideGizmo)
            return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + (_direction * 3F));
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one);

        // Using the render color from the prefab
        if(_miniCubePrefab != null) {
            Gizmos.color = _miniCubePrefab.GetComponent<Renderer>().sharedMaterial.color;
        } else {
            Gizmos.color = Color.blue;
        }
        
        Gizmos.DrawCube(transform.position, Vector3.one / 2);
    }

    private IEnumerator CubeInstantiator() {
        while (_canSpawn) {
            GameObject _object = Instantiate(_miniCubePrefab, transform.position, Quaternion.identity);
            _object.name = "miniCube";

            // Remove cubes when full...
            // Might not work when reducing size of the list ...
            if (_createdCubes.Count >= _maxObject) {
                Destroy(_createdCubes[0].gameObject);
                _createdCubes.RemoveAt(0);
            }
            _createdCubes.Add(_object);

            // set spawner as parent
            _object.transform.parent = transform;

            // add force
            _object.GetComponent<Rigidbody>().velocity = thrust * transform.localScale.x * _direction;

            // waiting until spawning next object
            yield return new WaitForSeconds(_timeGap);
        }
    }
}