using Home.Common.Model;
using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public class GreetingsMessage : BaseMessage
    {
        public const string SimpleGetGreetings = "greetings.get.simple";

        public enum GreetingsDestination
        {
            UserApp,
            Screen,
            Speakers
        }

        public GreetingsMessage(string messageTopic) : base(messageTopic)
        {
            Users = new List<User>();
        }

        public List<User> Users { get; set; }

        public GreetingsDestination Destination { get; set; }
    }

    public class GreetingsMessageResponse : MessageResponse
    {
        public GreetingsMessageResponse() : base()
        {
            Items = new List<GreetingsMessageResponseItem>();
        }

        public List<GreetingsMessageResponseItem> Items { get; set; }


        private class MyParagraphExtension : IMarkdownExtension
        {
            public void Setup(MarkdownPipelineBuilder pipeline)
            {
                // Do I need to implement this?
            }

            public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
            {
                renderer.ObjectRenderers.RemoveAll(x => x is ParagraphRenderer);
                renderer.ObjectRenderers.Add(new MyParagraphRenderer());
            }
        }

        private class MyParagraphRenderer : ParagraphRenderer
        {
            protected override void Write(HtmlRenderer renderer, ParagraphBlock obj)
            {
                if (obj.Parent is MarkdownDocument)
                {
                    if (!renderer.IsFirstInContainer)
                    {
                        renderer.EnsureLine();
                    }
                    renderer.WriteLeafInline(obj);
                    if (!renderer.IsLastInContainer)
                    {
                        renderer.WriteLine("<br />");
                        renderer.WriteLine("<br />");
                    }
                    else
                    {
                        renderer.EnsureLine();
                    }
                }
                else
                {
                    base.Write(renderer, obj);
                }
            }
        }

        public void ConvertTo(string convertTo)
        {
            if (string.IsNullOrEmpty(convertTo))
                return;
            if (Items == null || Items.Count == 0)
                return;


            switch (convertTo.ToLowerInvariant())
            {
                case "html":
                    var builder = new MarkdownPipelineBuilder();
                    builder.Extensions.Add(new MyParagraphExtension());
                    var pipeline = builder.Build();
                    
                    foreach (var r in Items)
                    {
                        if(!string.IsNullOrEmpty(r.Content))
                            r.Content = Markdown.ToHtml(r.Content, pipeline);
                    }
                    break;
            }
        }
    }

    public class GreetingsMessageResponseItem
    {
        public enum GreetingsMessageResponseItemKind
        {
            HeaderContent,
            MainContent,
            DateContent,
        }

        public string Content { get; set; }

        public string ContentUrl { get; set; }

        public GreetingsMessageResponseItemKind ContentKind { get; set; }
    }

}
