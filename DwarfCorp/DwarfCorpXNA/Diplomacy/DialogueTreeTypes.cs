using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DwarfCorp.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DwarfCorp.Dialogue
{
    public class DialogueContext
    {
        private Action<DialogueContext> NextAction = null;
        public Gum.Widget ChoicePanel;
        public Gum.Widget SpeechBubble;
        public NewGui.TradePanel TradePanel;
        public Animation SpeakerAnimation;
        private Timer SpeechTimer = new Timer(0.0f, true);

        public Faction.TradeEnvoy Envoy;
        public String EnvoyName = "TODO";

        public Faction PlayerFaction;

        public Diplomacy.Politics Politics;
        public WorldManager World;

        public void Say(String Text)
        {
            SpeechBubble.Text = Text;
            SpeechBubble.Invalidate();

            SpeakerAnimation.Loop();
            SpeechTimer.Reset(Text.Length * 0.1f);
        }

        public void ClearOptions()
        {
            ChoicePanel.Clear();
        }

        public void AddOption(String Prompt, Action<DialogueContext> Action)
        {
            ChoicePanel.AddChild(new Gum.Widget
            {
                Text = Prompt,
                OnClick = (sender, args) => Transition(Action),
                AutoLayout = Gum.AutoLayout.DockTop,
                Font = "font-hires",
                TextColor = Color.Black.ToVector4(),
                ChangeColorOnHover = true,
                HoverTextColor = Color.DarkRed.ToVector4()
            });

            ChoicePanel.Layout();
        }

        public void Transition(Action<DialogueContext> NextAction)
        {
            this.NextAction = NextAction;
        }

        public void Update(DwarfTime Time)
        {
            SpeechTimer.Update(Time);
            if (SpeechTimer.HasTriggered)
                SpeakerAnimation.Stop();
            else
                SpeakerAnimation.Update(Time);

            var next = NextAction;
            NextAction = null;

            if (next != null)
                next(this);


        }
    }    
}