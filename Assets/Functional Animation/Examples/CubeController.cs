using UnityEngine;

namespace Aikom.FunctionalAnimation.Examples
{
    public class CubeController : MonoBehaviour
    {   
        private enum JumpMode
        {
            Physics,
            Animation
        }

        [SerializeField] private float _speed = 10;
        [SerializeField] private float _jumpHeight = 5;
        [SerializeField] private JumpMode _jumpMode;

        private ScriptableTransformAnimator _animator;
        private RuntimeController _controller;
        private static readonly int _jumpHash = "Cube Jump".GetHashCode();
        private static readonly int _jumpImpactHash = "Cube Impact Soft".GetHashCode();
        private bool _isGrounded;
        private Rigidbody _rb;
        
        private void Start()
        {
            _animator = GetComponent<ScriptableTransformAnimator>();
            _rb = GetComponent<Rigidbody>();
            _controller = _animator.RuntimeController;
        }

        private void Update()
        {   
            // Listen for spacebar input
            if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
            {   
                // Resets the scale back to default values
                _animator.ResetProperty(TransformProperty.Scale);

                if(_jumpMode == JumpMode.Physics)
                {
                    _rb.AddForce(Vector3.up * Mathf.Sqrt(_jumpHeight * -2f * Physics.gravity.y), ForceMode.VelocityChange);

                    // This loads the animation data from graph and initializes the controller
                    _animator.Play(_jumpImpactHash);
                }
                else
                {   
                    // This loads the animation data from graph and initializes the controller
                    _animator.Play(_jumpHash);

                    // Once the animation is playing you can freely modify its current target values
                    _controller.OverrideTarget(TransformProperty.Position, Axis.Y, _jumpHeight);
                }
                _isGrounded = false;
            }

            // Horizontal movement. Since the jump animation is specifically set to modify only 
            // the Y axis, the cube is free to move how ever it wants on the X axis
            var x = Input.GetAxisRaw("Horizontal");
            transform.position += new Vector3(x, 0, 0) * Time.deltaTime * _speed;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Ground") && !_isGrounded)
            {
                _animator.Play(_jumpImpactHash);
                _isGrounded = true;
            }
        }
    }
}
