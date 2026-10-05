using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace MelodySuite.Outfit.Runtime
{
    public class OutfitController : MonoBehaviour
    {
        [SerializeField]
        private List<Outfit> m_outfits = new();
    
        private Dictionary<string, Outfit> outfitLookup;
        
        public IReadOnlyList<Outfit> outfits => m_outfits.AsReadOnly();
    
        [CanBeNull] public Outfit GetOutfit(string id) => outfitLookup.TryGetValue(id, out var outfit) ? outfit : null;
    
        [CanBeNull] public Outfit GetFirstActiveOutfit() => outfits.FirstOrDefault(outfit => outfit.active);

        private void Init()
        {
            outfitLookup = new Dictionary<string, Outfit>();
            foreach (var outfit in m_outfits)
            {
                if (!outfitLookup.TryAdd(outfit.ID, outfit))
                    Debug.LogError($"Could not add outfit with key {outfit.ID}.");
            
                outfit.Init(this);
            }
        }

        public void SetActiveOutfit(Outfit outfit)
        {
            foreach (var o in m_outfits)
                o.active = false;
            outfit.active = true;
        }
        
        [ContextMenu("ApplyAllOutfits")]
        public void ApplyAllOutfits()
        {
            var targetLookup = new Dictionary<Transform, List<OutfitPart>>();
    
            foreach (var part in m_outfits.SelectMany(outfit => outfit.parts.Where(part => part.target)))
            {
                if (!targetLookup.ContainsKey(part.target))
                    targetLookup[part.target] = new List<OutfitPart>();

                targetLookup[part.target].Add(part);
            }
            
            foreach (var (target, parts) in targetLookup)
            {
                var shouldBeActive = parts.Any(p => p.parent.active && p.active);
                target.gameObject.SetActive(shouldBeActive);
            }
        }

        private void OnValidate()
        {
            Init();
        }
    }
    
    [System.Serializable]
    public class Outfit
    {

        [SerializeField] private string id; 
        [SerializeField] private bool isActive = true;
        
        [SerializeField] private List<OutfitPart> m_parts = new();
    
        private Dictionary<string, OutfitPart> _partLookup;

        [NonSerialized]
        private OutfitController _controller;
        
        public string ID => id;
    
        public List<OutfitPart> parts => m_parts;
    
        internal void Init(OutfitController c)
        {
            _controller = c;
            
            _partLookup = new Dictionary<string, OutfitPart>();
            foreach (var part in m_parts)
            {
                part.Init(c, this);
                if (!_partLookup.TryAdd(part.ID, part))
                    Debug.LogError($"Could not add outfit part with key {part.ID}.");
            }
        }
    
        public bool active
        {
            get => isActive;
        
            set
            {
                isActive = value;
                _controller.ApplyAllOutfits();
            }
        }
    }

    [System.Serializable]
    public class OutfitPart
    {
        [SerializeField] private string id;
        [SerializeField] internal Transform target;
        [SerializeField] private bool isActive;

        [NonSerialized]
        private Outfit _parent;
        [NonSerialized]
        private OutfitController _controller;

        public Outfit parent => _parent;
        
        public string ID => id;
        
        internal void Init(OutfitController c, Outfit outfit)
        {
            _parent = outfit;
            _controller = c;
        }
        
        public bool active
        {
            get => isActive;
            set
            {
                isActive = value;
                _controller.ApplyAllOutfits();
            }
        }
    }
}