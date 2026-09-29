using Bunit;
using CommunityLink.App.Components.Shared.Chat;
using AngleSharp.Dom;

namespace CommunityLink.App.Tests.Components;

/// <summary>
/// Renders <see cref="ChatThreadList"/> the way the /chat page does and asserts the
/// thread-search box never leaks Razor markup into the DOM.
/// </summary>
public class ChatThreadListSearchBoxTests : BunitContext
{
    private IRenderedComponent<ChatThreadList> RenderDefault() =>
        Render<ChatThreadList>(p => p
            .Add(x => x.IsLoading, false)
            .Add(x => x.HasError, false));

    [Fact]
    public void SearchBox_HasNoValueAttribute_WhenSearchTextIsNull()
    {
        var cut = RenderDefault();

        var input = cut.Find("#chat-thread-search");

        Assert.Null(input.GetAttribute("value"));
    }

    [Fact]
    public void SearchBox_DoesNotRenderLiteralSearchText()
    {
        var cut = RenderDefault();

        // The literal word "searchText" must never reach the browser, either as the
        // input value or as stray text anywhere in the component output.
        Assert.DoesNotContain("searchText", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Renders_NoStrayRazorBrace_TextNodes()
    {
        var cut = RenderDefault();

        // A mismatched brace previously leaked through AddMarkupContent and rendered
        // as a literal "}" on the page. Assert the rendered text is brace-free.
        var text = cut.FindAll("*")
            .Where(n => n.NodeType == NodeType.Text)
            .Select(n => n.NodeValue ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain(text, t => t.Contains('}'));
    }

    [Fact]
    public void SearchBox_ReflectsProvidedSearchText()
    {
        var cut = Render<ChatThreadList>(p => p
            .Add(x => x.IsLoading, false)
            .Add(x => x.HasError, false)
            .Add(x => x.SearchText, "alice"));

        Assert.Equal("alice", cut.Find("#chat-thread-search").GetAttribute("value"));
    }

    [Fact]
    public void SearchBox_RaisesOnSearchChanged_WithTypedValue()
    {
        var captured = new List<string?>();

        var cut = Render<ChatThreadList>(p => p
            .Add(x => x.IsLoading, false)
            .Add(x => x.HasError, false)
            .Add(x => x.OnSearchChanged, s => captured.Add(s)));

        cut.Find("#chat-thread-search").Input("bob");

        Assert.Equal(new[] { "bob" }, captured);
    }

    [Fact]
    public void DiscoverDrawer_SearchBox_HasNoValue_WhenSearchTextIsNull()
    {
        var cut = Render<DiscoverGroupsDrawer>(p => p
            .Add(x => x.IsLoading, false));

        Assert.Null(cut.Find("#discover-group-search").GetAttribute("value"));
    }

    [Fact]
    public void DiscoverDrawer_DoesNotRenderLiteralSearchText()
    {
        var cut = Render<DiscoverGroupsDrawer>(p => p
            .Add(x => x.IsLoading, false));

        Assert.DoesNotContain("searchText", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }
}
