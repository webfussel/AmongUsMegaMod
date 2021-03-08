using System;
using System.Collections.Generic;
using UnityEngine;

namespace MegaMod
{
    public class CooldownButton
    {
        private static List<CooldownButton> buttons = new List<CooldownButton>();
     
        private readonly Action OnClick;
        private readonly Action OnUpdate;
        private readonly HudManager hudManager;
        private readonly Sprite sprite;

        public Vector2 PositionOffset = Vector2.zero;
        public float MaxTimer = 0f;
        public float Timer = 0f;
        public bool enabled = true;       
        public KillButtonManager killButtonManager;
        public bool canUse;

        public CooldownButton(Action OnClick, float Cooldown, Vector2 PositionOffset, HudManager hudManager, Sprite sprite, Action OnUpdate)
        {
            this.hudManager = hudManager;
            this.OnClick = OnClick;
            this.OnUpdate = OnUpdate;
            this.PositionOffset = PositionOffset;
            this.sprite = sprite;
            MaxTimer = Cooldown;
            Timer = MaxTimer;
            canUse = true; // TODO: Muss noch auf false gesetzt werden, wenn Spieler stirbt
            buttons.Add(this);
            Start();
        }

        private void Start()
        {
            killButtonManager = UnityEngine.Object.Instantiate(hudManager.KillButton, hudManager.transform);
            killButtonManager.gameObject.SetActive(true);
            killButtonManager.transform.position += (Vector3) PositionOffset;
            killButtonManager.renderer.enabled = true;
            killButtonManager.renderer.sprite = sprite;
            PassiveButton button = killButtonManager.GetComponent<PassiveButton>();
            button.OnClick.RemoveAllListeners();
            button.OnClick.AddListener((UnityEngine.Events.UnityAction)listener);

            void listener()
            {
                if (Timer < 0f && canUse)
                {
                    killButtonManager.renderer.color = new Color(1f, 1f, 1f, 0.3f);
                    Timer = MaxTimer;
                    OnClick();
                }
            }
        }

        public static void HudUpdate()
        {
            buttons.RemoveAll(item => item.killButtonManager == null);
            foreach (CooldownButton button in buttons)
            {
                button.OnUpdate();
                button.Update();
            }
        }

        private void Update()
        {
            //if (killButtonManager.transform.localPosition.x > 0f)
                //killButtonManager.transform.localPosition = new Vector3((killButtonManager.transform.localPosition.x + 1.3f) * -1, killButtonManager.transform.localPosition.y, killButtonManager.transform.localPosition.z) + new Vector3(PositionOffset.x, PositionOffset.y);
            if (Timer < 0f)
            {
                killButtonManager.renderer.color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                if (canUse &&  PlayerControl.LocalPlayer.CanMove)
                    Timer -= Time.deltaTime;

                killButtonManager.renderer.color = new Color(1f, 1f, 1f, 0.3f);
            }
            killButtonManager.gameObject.SetActive(canUse);
            killButtonManager.renderer.enabled = canUse;
            if (canUse)
            {
                killButtonManager.renderer.material.SetFloat("_Desat", 0f);
                killButtonManager.SetCoolDown(Timer, MaxTimer);
            }
        }
    }
}
