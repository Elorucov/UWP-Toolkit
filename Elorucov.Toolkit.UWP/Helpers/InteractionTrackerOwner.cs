using System;
using System.Numerics;
using Windows.UI.Composition.Interactions;

namespace Elorucov.Toolkit.UWP.Helpers {
    internal enum InteractionTrackerState {
        None, CustomAnimation, Idle, Inertia, Interacting, RequestIgnored
    }

    internal class InteractionTrackerOwner : IInteractionTrackerOwner {

        Vector3 position;
        public InteractionTrackerState State { get; private set; }

        public void CustomAnimationStateEntered(InteractionTracker sender, InteractionTrackerCustomAnimationStateEnteredArgs args) {
            State = InteractionTrackerState.CustomAnimation;
            StateOrValuesChanged?.Invoke(sender, new Tuple<InteractionTrackerState, Vector3>(State, position));
        }

        public void IdleStateEntered(InteractionTracker sender, InteractionTrackerIdleStateEnteredArgs args) {
            State = InteractionTrackerState.Idle;
            StateOrValuesChanged?.Invoke(sender, new Tuple<InteractionTrackerState, Vector3>(State, position));
        }

        public void InertiaStateEntered(InteractionTracker sender, InteractionTrackerInertiaStateEnteredArgs args) {
            State = InteractionTrackerState.Inertia;
            StateOrValuesChanged?.Invoke(sender, new Tuple<InteractionTrackerState, Vector3>(State, position));
        }

        public void InteractingStateEntered(InteractionTracker sender, InteractionTrackerInteractingStateEnteredArgs args) {
            State = InteractionTrackerState.Interacting;
            StateOrValuesChanged?.Invoke(sender, new Tuple<InteractionTrackerState, Vector3>(State, position));
        }

        public void RequestIgnored(InteractionTracker sender, InteractionTrackerRequestIgnoredArgs args) {
            State = InteractionTrackerState.RequestIgnored;
            StateOrValuesChanged?.Invoke(sender, new Tuple<InteractionTrackerState, Vector3>(State, position));
        }

        public void ValuesChanged(InteractionTracker sender, InteractionTrackerValuesChangedArgs args) {
            position = args.Position;
            StateOrValuesChanged?.Invoke(sender, new Tuple<InteractionTrackerState, Vector3>(State, position));
        }

        public event EventHandler<Tuple<InteractionTrackerState, Vector3>> StateOrValuesChanged;
    }
}
