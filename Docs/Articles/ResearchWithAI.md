
The following analysis is answered by Claude.ai.

# XLIFF inline elements

In the XLIFF 1.2 spec, these are officially called **inline elements**.

The spec (OASIS XLIFF 1.2) defines a family of them that can appear inside `<source>`/`<target>`: `<g>`, `<x/>`, `<bx/>`, `<ex/>`, `<ph>`, `<it>`, `<mrk>`. Together they're referred to as **"inline elements"** or sometimes **"inline markup"**.

Specifically, `<x/>` is called the **"x" (generic placeholder) element**, formally described as representing *"a single, standalone code of the original document, with no corresponding content"* — i.e. it's a placeholder for native/non-textual content that must be preserved in position but not translated. The spec's own summary:

- **`<x/>`** – standalone placeholder, no content, no translatable text (this is what Angular emits for interpolations, ICU expressions, etc.)
- **`<bx/>` / `<ex/>`** – paired begin/end placeholders for a native code span that isn't well-formed XML (e.g. an unpaired formatting tag)
- **`<g>...</g>`** – a paired inline element that *does* wrap translatable content (e.g. `<b>`/`<span>` equivalents)
- **`<ph>`** – standalone placeholder that *can* carry sub-flow content (rarely used by Angular)
- **`<it>`** – "isolated tag," a half of a pair where only one side appears in this segment
- **`<mrk>`** – used for annotations/comments, not code placeholders

So in your case: `<x id="PH" equiv-text="numberList"/>` is an **XLIFF inline placeholder element**, specifically the `<x/>` (generic placeholder) inline element, and `equiv-text` is an XLIFF 1.2 optional attribute giving a human-readable stand-in for what that placeholder will resolve to at runtime (useful for translators/CAT tools to show context without rendering the actual code).

Angular's i18n extraction tool generates one `<x id="...">` per interpolation, ICU expression, or HTML tag inside a translatable string, precisely so translators can reposition the placeholder in the target language without needing to know or touch the underlying Angular template syntax.

# Translation with inline elements included

Good question — this gets at a real and often underappreciated quality issue in MT pipelines. Short answer: **`translateHtml()` on the whole inner XML will almost always produce better, more natural translations than splitting into separate `translate(plainText)` calls and concatenating.** Here's why, and where the risks are.

## Why splitting hurts translation quality

When you call `translate(plainText)` twice — once for `"There are some registered numbered annotations not existing in poem anymore: "` and once for `". Do you want to remove them?"` — you're asking the engine to translate two **sentence fragments in isolation**, each missing crucial context:

1. **Loss of syntactic context across the placeholder.** The first fragment ends mid-sentence with a colon, and the second starts with a period and a new sentence. Many languages restructure sentences in translation (reordering clauses, moving verbs, changing word order entirely — this is especially true for English→Chinese, English→Japanese, English→German, etc.). If the engine doesn't know that "numberList" (a noun) is going to be dropped into the middle of the first fragment, it can't correctly conjugate, place particles, or reorder around it. You saw this yourself in your Chinese example — the placeholder sits before "你想删除它们吗" and the translation had to grammatically anticipate a noun there.

2. **Broken sentence boundary detection.** MT engines use sentence-boundary and discourse-level context (sometimes even attention across the whole paragraph) to disambiguate meaning, pick correct pronouns, tense, formality register, etc. Splitting a single logical sentence into two independent calls removes that shared context. The two halves are translated as if they were unrelated inputs, so you can end up with mismatched tone, redundant subject reinsertion, or awkward fragment grammar that doesn't recombine naturally.

3. **Fragment 2 in particular is dangerous.** `". Do you want to remove them?"` starts with a bare period and lowercase-continuation punctuation with no subject context of "them" — the engine may not even reliably know "them" refers to "annotations" from the first fragment, since it's not present in that call at all.

## Why `translateHtml()` is designed for exactly this

Both Google Cloud Translation and Microsoft Translator's HTML-aware endpoints exist specifically to solve the "inline markup / placeholder" problem CAT tools and localization pipelines have always had:

- They translate the **full sentence as one coherent unit**, giving the engine complete context for word order, agreement, and register.
- They treat recognized inline tags (like your `<x/>`) as **protected, non-translatable spans** — the engine translates around them and repositions the tag according to the target language's grammar, rather than you having to manually decide where to splice the placeholder back in.
- Attributes like `id`, `equiv-text` are preserved as opaque metadata; the tag itself is passed through untranslated but treated as a real token in the sentence for reordering purposes.

This is essentially the same reason XLIFF itself defines inline elements — the whole ecosystem (CAT tools, MT engines, TMS platforms) is built around "translate this as one unit with protected islands," not "translate the pieces and stitch them back together."

## Practical caveats

- **Not all inline tags are auto-protected.** Google's `translateHtml` / MS Translator's HTML mode generally protect standard HTML tags (`<b>`, `<span>`, `<a>`, etc.) reliably, but a custom/non-HTML tag like `<x id="PH" equiv-text="numberList"/>` may or may not be recognized as "do not translate, do not touch content" depending on the engine and API version. Test this specifically — if the engine tries to "translate" text inside odd attributes or mishandles a self-closing tag it doesn't recognize, you could get corrupted output.
- **Safer common practice:** many localization pipelines substitute XLIFF inline tags with a neutral HTML tag the MT engine is guaranteed to protect — e.g. replace `<x id="PH" .../>` with `<span translate="no" data-ph-id="PH"></span>` (or even a simple `<0></0>`-style numbered tag, which Microsoft's API explicitly supports) before calling `translateHtml`, then map it back to the original XLIFF `<x/>` afterward. This gives you HTML-mode's contextual benefits while guaranteeing the placeholder survives untouched.
- **Multiple placeholders**, if present, also need each to map to a distinct protected tag so the engine can independently reposition each one — don't collapse them to identical tags or the engine may not track which is which.

## Bottom line

Pass the whole inner XML through `translateHtml()` (with your inline tags substituted for a format the engine reliably protects, if needed) rather than splitting into plain-text fragments and concatenating. You'll get better word order, agreement, and naturalness, especially for languages that restructure sentences significantly relative to English — exactly the class of problem you're already seeing with the Chinese example you posted.

# Google Translate v2 or v3 for translating from English to Chinese

That actually explains it pretty well. UI and help content is exactly the case where an unpinned "smart" default model can hurt you — short strings, button labels, menu items, and instructional sentences need to be literal, consistent, and terminology-stable, not stylistically loosened. If v3's default is resolving to the TLLM path rather than NMT, you'd get more natural-sounding prose in isolation, but potentially:

- Inconsistent phrasing for the same source string appearing in different contexts (bad for UI, where "Cancel" or "Save changes" needs to translate identically everywhere)
- Looser handling of technical terms it doesn't have hard constraints on
- More paraphrasing/interpretation on short imperative sentences, which for a native speaker can read as *less* accurate even if more fluent

That lines up with what one reviewer described — NMT for functional text like UI strings, LLM-style models reserved for prose meant to read naturally. Your case is squarely the former.

Two things worth trying:

1. **Pin `model: nmt` explicitly in your v3 calls.** That should get you the same base model v2 was using, and let you re-test whether v2 vs v3 is actually still different once you're comparing like-for-like.
2. **If you stay on v3, consider a glossary.** For UI/help content, a termbase locking down your product's key terms (menu names, feature names, action verbs) will likely do more for consistency than switching models — v3's glossary support doesn't exist in v2 at all, so it's a real advantage once terminology is the priority rather than "naturalness."

If you pin NMT on v3 and still see v2 outperforming it on identical Chinese output, that'd be a genuinely interesting discrepancy worth reporting with examples, since Google's docs treat that as the same underlying model either way — but my guess is the default routing is the actual culprit here, not v3 being categorically worse at Chinese.

# NMT vs LLM

That's a great concrete example, and it's a textbook illustration of the exact risk I flagged earlier: with an isolated single word and no surrounding context, TLLM is behaving less like "translate this string" and more like "here's a dictionary entry for this word," hedging across multiple senses instead of committing to one. NMT, by contrast, just picks the statistically most probable single output and returns it — which is exactly the behavior you want for a UI string that has to fit in a button or label.

This is a real and important distinction for your use case:

- **NMT** treats translation as a sequence-to-sequence task — one input, one output, no ambiguity-hedging.
- **TLLM**, being LLM-based, has more "awareness" that a bare word like "Deficiency" is polysemous (medical deficiency, vitamin deficiency, character/moral deficiency, structural deficiency), and without a sentence or surrounding context to disambiguate, it's essentially giving you its top few candidate senses rather than picking one.

For software UI strings specifically, this makes `general/nmt` the more practically usable choice, independent of which one is "more accurate" in the abstract — a UI label can't render "缺乏；不足；缺陷", it needs exactly one string.

If you ever do want to try TLLM for longer help-content prose (where it may genuinely translate more naturally per the earlier benchmark claims), one mitigation for the multi-option problem is giving it more context per string — e.g., translating full sentences rather than isolated terms, or wrapping bare UI keys in a minimal sentence template before sending them, then stripping the template back out. But for short, standalone UI strings like your example, I'd stick with NMT — it's not just "more accurate" by your earlier read, it's structurally the right tool for space-constrained, single-answer-required strings.