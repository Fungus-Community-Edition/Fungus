using UnityEngine;
using UnityEngine.Events;
using AtMycelia.Physics;

namespace AtMycelia.Events
{
    public class PhysicsEventNotifier : MonoBehaviour, ICollisionEventNotifier
    {
        [Tooltip("This notifier will only notify collision events with objects on any of the layers specified here.")]
        [SerializeField] protected LayerMask _layerMask = ~0;

        #region TwoDim
        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.IsOnLayer(_layerMask)) { return; }
            TriggerEnter2D(other);
        }

        public event UnityAction<Collider2D> TriggerEnter2D = delegate { };

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            if (!other.IsOnLayer(_layerMask)) { return; }
            TriggerExit2D(other);
        }

        public event UnityAction<Collider2D> TriggerExit2D = delegate { };

        protected virtual void OnTriggerStay2D(Collider2D other)
        {
            if (!other.IsOnLayer(_layerMask)) { return; }
            TriggerStay2D(other);
        }

        public event UnityAction<Collider2D> TriggerStay2D = delegate { };

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.IsOnLayer(_layerMask)) { return; }
            CollisionEnter2D(collision);
        }

        public event UnityAction<Collision2D> CollisionEnter2D = delegate { };

        protected virtual void OnCollisionExit2D(Collision2D collision)
        {
            if (!collision.IsOnLayer(_layerMask)) { return; }
            CollisionExit2D(collision);
        }

        public event UnityAction<Collision2D> CollisionExit2D = delegate { };

        protected virtual void OnCollisionStay2D(Collision2D collision)
        {
            if (!collision.IsOnLayer(_layerMask)) { return; }
            CollisionStay2D(collision);
        }

        public event UnityAction<Collision2D> CollisionStay2D = delegate { };

        #endregion

        #region ThreeDim
        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!other.IsOnLayer(_layerMask)) { return; }
            TriggerEnter(other);
        }

        public event UnityAction<Collider> TriggerEnter = delegate { };

        protected virtual void OnTriggerExit(Collider other)
        {
            if (!other.IsOnLayer(_layerMask)) { return; }
            TriggerExit(other);
        }

        public event UnityAction<Collider> TriggerExit = delegate { };

        protected virtual void OnTriggerStay(Collider other)
        {
            if (!other.IsOnLayer(_layerMask)) { return; }
            TriggerStay(other);
        }

        public event UnityAction<Collider> TriggerStay = delegate { };

        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (!collision.IsOnLayer(_layerMask)) { return; }
            CollisionEnter(collision);
        }

        public event UnityAction<Collision> CollisionEnter = delegate { };

        protected virtual void OnCollisionExit(Collision collision)
        {
            if (!collision.IsOnLayer(_layerMask)) { return; }
            CollisionExit(collision);
        }

        public event UnityAction<Collision> CollisionExit = delegate { };

        protected virtual void OnCollisionStay(Collision collision)
        {
            if (!collision.IsOnLayer(_layerMask)) { return; }
            CollisionStay(collision);
        }

        public event UnityAction<Collision> CollisionStay = delegate { };
        #endregion

        protected virtual void OnParticleCollision(GameObject other)
        {
            if (!other.IsOnLayer(_layerMask)) { return; }
            ParticleCollision(other);
        }

        public event UnityAction<GameObject> ParticleCollision = delegate { };
    }
}