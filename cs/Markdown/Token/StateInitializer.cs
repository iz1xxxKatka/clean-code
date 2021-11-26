using System;
using System.Collections.Generic;

namespace Markdown
{
    public class StateInitializer
    {
        private readonly TokenProvider provider;
        private State startState;
        private State shieldingState;
        private State italicsState;
        private State boldState;
        private State imageState;
        private State headingState;

        private StateInitializer(TokenProvider provider)
        {
            this.provider = provider;
        }

        public static StateInitializer Create(TokenProvider tokenProvider) => new StateInitializer(tokenProvider);

        public StateInitializer Initialize()
        {
            CreateStates();
            InitStates();
            return this;
        }

        public State GetStartState() => startState;

        private void CreateStates()
        {
            startState = State.Create();
            shieldingState = State.Create();
            italicsState = State.Create();
            boldState = State.Create();
            imageState = State.Create();
            headingState = State.Create();
        }

        private void InitStates()
        {
            InitStartState();
            InitShieldingState();
            InitItalicsState();
            InitBoldState();
            InitImageState();
            InitHeadingState();
        }

        private void InitStartState()
        {
            startState
                .AddTransition('~', imageState)
                .AddTransition('\\', shieldingState)
                .AddTransition('_', italicsState)
                .AddTransition('\n', headingState)
                .SetFallback(startState);
        }

        private void InitShieldingState()
        {
            shieldingState
                .SetFallback(startState)
                .SetOnEntry((s, i) =>
                {
                    if (s[i + 1] != '_' && s[i + 1] != '\\') return;
                    var shieldingTokens = provider.GetStack(typeof(ShieldingTag));
                    shieldingTokens.Push(new Token(i, new ShieldingTag()) {Length = 1});
                });
        }

        private void InitItalicsState()
        {
            italicsState
                .AddTransition('~', imageState)
                .AddTransition('_', boldState)
                .AddTransition('\\', shieldingState)
                .AddTransition('\n', headingState)
                .SetFallback(startState)
                .SetOnEntry((s, i) =>
                {
                    if (s[i + 1] == '_')
                        return;
                    var italicsTokens = provider.GetStack(typeof(ItalicsTag));
                    if (!italicsTokens.TryPeek(out var italicsToken))
                        italicsTokens.Push(new Token(i, new ItalicsTag()));
                    else if (IsReadyToken(italicsToken))
                        italicsTokens.Push(new Token(i, new ItalicsTag()));
                    else if (IsNotInnerItalicsToken(italicsToken))
                        italicsToken.Length = i + 1 - italicsToken.StartPosition;
                });
        }

        private bool IsNotInnerItalicsToken(Token italicsToken)
        {
            return !provider.GetStack(typeof(BoldTag)).TryPeek(out var boldToken)
                   || boldToken.StartPosition <= italicsToken.StartPosition;
        }

        private static bool IsReadyToken(Token token)
        {
            return token.Length != 0;
        }

        private void InitBoldState()
        {
            boldState
                .AddTransition('~', imageState)
                .AddTransition('_', italicsState)
                .AddTransition('\\', shieldingState)
                .AddTransition('\n', headingState)
                .SetFallback(startState)
                .SetOnEntry((s, i) =>
                {
                    var boldTokens = provider.GetStack(typeof(BoldTag));
                    if (!boldTokens.TryPeek(out var boldToken))
                        boldTokens.Push(new Token(i - 1, new BoldTag()));
                    else if (IsReadyToken(boldToken))
                        boldTokens.Push(new Token(i - 1, new BoldTag()));
                    else if (IsInnerBoldToken())
                        boldTokens.Pop();
                    else
                        boldToken.Length = i - boldToken.StartPosition + 1;
                });
        }

        private bool IsInnerBoldToken()
        {
            return provider.GetStack(typeof(ItalicsTag)).TryPeek(out var italicsToken)
                   && !IsReadyToken(italicsToken);
        }

        private void InitImageState()
        {
            imageState
                .AddTransition('\n', headingState)
                .AddTransition('\\', shieldingState)
                .AddTransition('_', italicsState)
                .SetFallback(startState)
                .SetOnEntry((s, i) =>
                {
                    var imageTokens = provider.GetStack(typeof(ImageTag));
                    if (!imageTokens.TryPeek(out var imageToken))
                        imageTokens.Push(new Token(i, new ImageTag()));
                    else
                    {
                        RemoveInnerTokens(typeof(ItalicsTag), imageToken);
                        RemoveInnerTokens(typeof(BoldTag), imageToken);
                        imageToken.Length = i - imageToken.StartPosition + 1;
                    }
                });
        }

        private void RemoveInnerTokens(Type innerTokenType, Token imageToken)
        {
            var tokens = provider.GetStack(innerTokenType);
            while (tokens.TryPeek(out var token) &&
                   token.StartPosition > imageToken.StartPosition)
                tokens.Pop();
        }

        private void InitHeadingState()
        {
            headingState
                .AddTransition('\\', shieldingState)
                .AddTransition('_', italicsState)
                .SetFallback(startState)
                .SetOnEntry((s, i) =>
                {
                    var headingTokens = provider.GetStack(typeof(HeadingTag));
                    headingTokens.TryPeek(out var token);
                    token.Length = i - token.StartPosition + 1;
                    headingTokens.Push(new Token(i + 1, new HeadingTag()) {Length = s.Length - i});
                });
        }
    }
}