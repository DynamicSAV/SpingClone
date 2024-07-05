using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private string _knotTag;
    [SerializeField] private float _motorSpeed;
    [SerializeField] private float _overlapRadius;
    [SerializeField] private float _knotLength;
    [SerializeField] private Color _lineColor = Color.white;
    [SerializeField] private float _lineWidth = 0.02f;

    private HingeJoint2D _hingeJoint2D;
    private bool _isConnect;
    private LineRenderer _lineRenderer;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _hingeJoint2D = GetComponent<HingeJoint2D>();
        _rb = GetComponent<Rigidbody2D>();
        _hingeJoint2D.enabled = false;

        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.startColor = _lineColor;
        _lineRenderer.endColor = _lineColor;
        _lineRenderer.widthMultiplier = _lineWidth;
        _lineRenderer.positionCount = 2;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !_isConnect)
        {
            TryConnectHook();
        } 
        else if (Input.GetMouseButtonDown(0))
        {
            TryRemoveHook();
        }
    }

    private void TryConnectHook()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, _overlapRadius);

        foreach (var hitCollider in hitColliders)
        {
            if (!hitCollider.gameObject.Equals(gameObject) && hitCollider.gameObject.CompareTag(_knotTag))
            {
                CheckDirectionAndAdjustMotorSpeed(hitCollider.gameObject.transform);
                Rigidbody2D hitRb = hitCollider.GetComponent<Rigidbody2D>();
                _hingeJoint2D.connectedBody = hitRb;
                Vector2 relativePosition = gameObject.transform.InverseTransformPoint(hitCollider.transform.position);
                _hingeJoint2D.anchor = relativePosition;
                _hingeJoint2D.enabled = true;
                
                _isConnect = true;
                break;
            }
        }
    }

    private void TryRemoveHook()
    {
        if (_hingeJoint2D.connectedBody)
        {
            _hingeJoint2D.connectedBody = null;
            _hingeJoint2D.enabled = false;

            _isConnect = false;
        }
    }

    private void CheckDirectionAndAdjustMotorSpeed(Transform staticPoint)
    {
        Vector2 directionToHitCollider = (staticPoint.position - transform.position).normalized;
        Vector2 currentVelocity = _rb.velocity.normalized;
        
        if (_rb.velocity.magnitude < 0.1f)
        {
            currentVelocity = transform.right;
        }
        
        float crossProduct = Vector3.Cross(currentVelocity, directionToHitCollider).z;

        JointMotor2D motor = _hingeJoint2D.motor;
        
        motor.motorSpeed = crossProduct > 0 ? -_motorSpeed : _motorSpeed;

        _hingeJoint2D.motor = motor;
    }



    private void LateUpdate()
    {
        if (_isConnect && _lineRenderer.positionCount >= 2)
        {
            _lineRenderer.SetPosition(0, transform.position);
            _lineRenderer.SetPosition(1, _hingeJoint2D.connectedBody.transform.position);
        }
        else
        {
            _lineRenderer.positionCount = _isConnect ? 2 : 0;
        }
    }
}
