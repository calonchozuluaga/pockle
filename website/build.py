"""Builds the static playpockle.com site into ./public from the page bodies below."""
import pathlib, shutil

ROOT = pathlib.Path(__file__).parent
OUT = ROOT / "public"
UPDATED = "November 8, 2026"
PRIVACY_UPDATED = "November 8, 2026"
COMPANY = "Zulapp LLC"
SUPPORT = "support@playpockle.com"
PRIVACY = "privacy@playpockle.com"
LEGAL = "legal@playpockle.com"


def mail(addr):
    return f'<a href="mailto:{addr}">{addr}</a>'


NAV = [("/support/", "Support"), ("/privacy/", "Privacy"), ("/terms/", "Terms")]


def page(path, title, description, body, current=None):
    nav = "".join(
        f'<a href="{href}"{" aria-current=page" if href == current else ""}>{label}</a>' for href, label in NAV
    )
    html = f"""<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>{title}</title>
<meta name="description" content="{description}">
<meta name="theme-color" content="#FBF8F5">
<meta property="og:title" content="{title}">
<meta property="og:description" content="{description}">
<meta property="og:image" content="https://playpockle.com/assets/og.png">
<link rel="icon" href="/assets/favicon.png">
<link rel="apple-touch-icon" href="/assets/apple-touch-icon.png">
<link rel="stylesheet" href="/assets/site.css">
</head>
<body>
<header class="top"><div class="wrap">
  <a class="mark" href="/" aria-label="Pockle home"><img src="/assets/wordmark.png" alt="Pockle" width="95" height="34"></a>
  <nav aria-label="Main">{nav}</nav>
</div></header>
<main>
{body}
</main>
<footer><div class="wrap">
  <div>© 2026 {COMPANY}. Pockle is a game by {COMPANY}.</div>
  <nav aria-label="Footer"><a href="/support/">Support</a><a href="/privacy/">Privacy Policy</a><a href="/terms/">Terms of Service</a><a href="/delete-account/">Delete your account</a></nav>
</div></footer>
</body>
</html>
"""
    target = OUT / path / "index.html" if path else OUT / "index.html"
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_text(html, encoding="utf-8")


def doc(path, title, description, colour, intro, sections, current=None, draft=True):
    toc = "".join(f'<a href="#{sid}">{heading}</a>' for sid, heading, _ in sections)
    content = "".join(f'<h2 id="{sid}">{heading}</h2>{html}' for sid, heading, html in sections)
    banner = (
        '<div class="note draft"><strong>Draft for legal review.</strong> This page was written to match how Pockle works '
        "today and has not yet been reviewed by a lawyer.</div>" if draft else ""
    )
    body = f"""<div class="doc-hero {colour}"><div class="wrap"><h1>{title}</h1><p>{intro}</p></div></div>
<div class="doc"><nav class="toc" aria-label="On this page">{toc}</nav><article>{banner}{content}</article></div>"""
    page(path, f"{title} · Pockle", description, body, current)


# ---------------------------------------------------------------- Home
# The home page is hand-written in home/index.html (styles in assets/home/home.css, images in assets/home/,
# re-exported from ArtSource/Website by home/build_assets.py). It is copied as is.
HOME_SOURCE = ROOT / "home" / "index.html"


# ---------------------------------------------------------------- Privacy
PRIV = [
    ("summary", "The short version", f"""
<ul>
<li>Pockle is a collecting game for people <strong>13 and older</strong>, made by {COMPANY}.</li>
<li>We collect what the game needs: your sign-in, your collector name, your toys, optional step counts and your purchases.</li>
<li>We <strong>don't</strong> collect your location, your contacts or your photos, and we don't keep your birthday.</li>
<li>We don't sell your personal information.</li>
<li>You can request deletion of your account in the app or <a href="/delete-account/">without the app</a>.</li>
</ul>"""),
    ("collect", "What we collect", f"""
<table class="plain"><tr><th>What</th><th>Why</th></tr>
<tr><td><strong>Sign-in details</strong> from Apple or Google: an account identifier and the name or email address returned by your sign-in provider (Apple may give us a private relay address instead).</td><td>To create your account and let you sign in on a new phone.</td></tr>
<tr><td><strong>Collector name</strong> and profile choices (avatar toy, favourite toy).</td><td>To identify your account and personalize your collection. We ask you not to use your real name.</td></tr>
<tr><td><strong>Age check result.</strong> You enter a birthday; we keep only whether you passed, not the date.</td><td>Pockle is for players 13 and older.</td></tr>
<tr><td><strong>Step counts</strong> from your phone's motion sensors (Android "Physical activity" or iPhone "Motion &amp; Fitness").</td><td>To unlock free boxes when you enable walking. Step totals and progress are included in cloud saves; we do not record where you walked.</td></tr>
<tr><td><strong>Game data:</strong> the toys and spares you own, boxes opened, fusions, badges, collection progress and saved preferences.</td><td>To run the game and keep your collection safe across devices.</td></tr>
<tr><td><strong>Purchase records</strong> from Apple or Google, processed through RevenueCat: product identifiers, transaction identifiers, purchase dates and purchase status, linked to your Pockle account.</td><td>To deliver boxes you pay for and prevent fraud. We never see your card details.</td></tr>
<tr><td><strong>Device and technical data:</strong> an app-generated installation identifier used with cloud saves, app and platform information used by purchase services, and service request logs used to operate accounts and purchases.</td><td>To link backups to an installation, deliver purchases and protect the service. The current beta does not send automatic crash reports or register a remote push notification token.</td></tr>
<tr><td><strong>Messages you send us</strong> at {SUPPORT} or {PRIVACY}.</td><td>To help you.</td></tr>
</table>
<p>We don't collect precise or approximate location, contacts, photos, microphone or camera data. Players can't send each other free-text messages.</p>"""),
    ("use", "How we use it", """
<ul><li>To provide accounts, cloud saves, collections, fusions and box delivery. Notification reminders in the current beta are scheduled on your phone.</li>
<li>To keep Pockle fair and safe: verifying steps and purchases, preventing cheating, enforcing our <a href="/terms/">Terms</a> and handling reports.</li>
<li>To understand purchases and delivery issues using purchase-service reports.</li>
<li>To meet legal obligations.</li></ul>
<p>We don't use your information for targeted advertising, and we don't sell it.</p>"""),
    ("public", "What other players can see", """
<p>The current beta does not publish your cloud save, shelf or trade offers to other players. Your collector name identifies your Pockle account; account email addresses, sign-in details and step history are not published.</p>
<p>If public shelves or player trading become available, we will describe what is visible and the controls available before enabling those features.</p>"""),
    ("share", "Who we share it with", f"""
<p>We share information only with service providers who help us run Pockle, under contracts that limit how they use it:</p>
<ul><li><strong>Supabase</strong> — database, accounts and game servers.</li>
<li><strong>RevenueCat</strong> — purchase processing, purchase validation and purchase reporting. It receives your Pockle account identifier and purchase history so we can deliver purchased boxes and resolve purchase issues.</li>
<li><strong>Apple and Google</strong> — sign-in, app distribution and in-app purchases. Their stores process payment details; Pockle does not receive your card number.</li>
<li><strong>Vercel</strong> — hosting for this website.</li></ul>
<p>We may also share information if the law requires it, to protect players' safety, or as part of a sale or merger of {COMPANY} (you would be told first).</p>"""),
    ("kids", "Children and teens", f"""
<p>Pockle is not directed to children under 13, and we don't knowingly collect personal information from them. The birthday check blocks anyone under 13 from creating an account.
If you believe a child under 13 has an account, contact {mail(PRIVACY)} and we will delete it.</p>
<p>Players aged 13 to 17 should have a parent or guardian's permission to play and to make purchases.</p>"""),
    ("keep", "How long we keep it", """
<p>We keep your information while your account is active. After a verified account-deletion request, we delete or de-identify associated account and cloud-save data within 30 days.
Purchase and transaction records may be retained where required for tax, accounting or fraud prevention. Deletion requests also cover personal data held by our service providers, subject to those retention requirements. Apple and Google separately control the purchase records in your store account.</p>"""),
    ("rights", "Your choices and rights", f"""
<ul><li><strong>Access, correct or delete</strong> your information: use the in-app settings, the <a href="/delete-account/">delete account page</a>, or email {mail(PRIVACY)}.</li>
<li><strong>Step counting</strong> and <strong>notifications</strong> can be turned off at any time in your phone's settings.</li>
<li>Depending on where you live (for example California or the European Union), you may have additional rights, such as to know what we hold, to object, or to port your data. Email {mail(PRIVACY)} and we'll respond within the time the law requires.</li></ul>"""),
    ("security", "Security", "<p>We use encryption in transit, access controls and server-side checks to protect your information. No system is perfectly secure, so please keep your Apple or Google account secure too.</p>"),
    ("changes", "Changes to this policy", "<p>If we make important changes, we'll update this page and tell you in the app before they take effect.</p>"),
    ("contact", "Contact us", f"<p>{COMPANY}<br>Privacy questions: {mail(PRIVACY)}<br>Everything else: {mail(SUPPORT)}</p>"),
]

# ---------------------------------------------------------------- Terms
TERMS = [
    ("agree", "Agreeing to these terms", f"""
<p>These Terms are an agreement between you and {COMPANY} ("we") about the Pockle app and playpockle.com. By creating an account or playing, you agree to them and to our <a href="/privacy/">Privacy Policy</a>.
If you are between 13 and 17, a parent or guardian must review and agree to these Terms with you.</p>"""),
    ("eligibility", "Who can play", "<p>You must be at least 13 years old. You may have one account, created with Sign in with Apple or Google. You're responsible for activity on your account.</p>"),
    ("items", "Toys, boxes and other virtual items", """
<ul><li>Toys, boxes, fusions and badges are virtual items. We grant you a limited, personal, non-transferable licence to use them in Pockle. They have no real-world monetary value and can't be exchanged for cash.</li>
<li>Boxes contain a randomly selected toy. The odds for every box are shown before you open or buy it. Some toys are made in limited numbers; when an edition runs out it can't be found anymore.</li>
<li>We may change, rebalance or retire items, but we won't take away toys you already own except under section "Ending your account" or to correct an error or fraud.</li></ul>"""),
    ("purchases", "Purchases", """
<p>You can buy boxes with real money through Apple's App Store or Google Play. Purchases are processed by them and are subject to their terms; refunds are handled through Apple or Google according to their policies and applicable law.
Purchases are final once the box is opened, except where the law says otherwise. Teens need a parent or guardian's permission to buy.</p>"""),
    ("trading", "Trading", """
<ul><li>Trades are toy-for-toy. Selling, buying or trading Pockle items or accounts for money or anything outside the game is not allowed.</li>
<li>Both players confirm the exact trade before it happens. Completed trades are final.</li>
<li>Toys used in a fusion are used up, and fused toys can't be traded.</li></ul>"""),
    ("walking", "Walking safely", "<p>Pockle rewards walking, but please pay attention to where you're going. Don't look at the app while crossing streets, driving or doing anything that needs your attention. Fake step counts (for example, shaking your phone or using a device to simulate steps) are cheating.</p>"),
    ("conduct", "Play fair and be kind", """
<p>Don't: cheat, exploit bugs, use bots or modified apps; choose offensive or misleading collector names; harass or scam other players; or try to access other accounts or our systems.
You can report or block any player in the app. We may remove names, reverse trades or items gained unfairly, and suspend accounts that break these rules.</p>"""),
    ("ending", "Ending your account", f"<p>You can delete your account at any time (see <a href=\"/delete-account/\">how</a>). We may suspend or close accounts that break these Terms. Deleting an account removes its toys permanently, and purchases are not refunded except where required by law.</p>"),
    ("ip", "Our content", f"<p>Pockle, its characters, art, sounds and software belong to {COMPANY} or its licensors. Please don't copy or redistribute them except to share screenshots and videos of your own play.</p>"),
    ("disclaimers", "Disclaimers and limits", f"""
<p>Pockle is provided "as is". We work to keep it running but don't promise it will always be available or error-free. To the extent the law allows, {COMPANY} isn't liable for indirect or consequential damages,
and our total liability is limited to the amount you paid us in the 12 months before the claim. Some places don't allow these limits, so they may not apply to you.</p>"""),
    ("law", "Governing law", f"<p>These Terms are governed by the laws of the State of Colorado, USA, without regard to conflict-of-law rules. Nothing here limits rights you have under the consumer laws of where you live.</p>"),
    ("changes", "Changes", "<p>We may update these Terms. For important changes we'll tell you in the app in advance; continuing to play after they take effect means you accept them.</p>"),
    ("contact", "Contact", f"<p>{COMPANY}<br>Legal notices: {mail(LEGAL)}<br>Help: {mail(SUPPORT)}</p>"),
]

# ---------------------------------------------------------------- Support
FAQ = [
    ("How do I get free boxes?", "Turn on walking in the app. Every 1,000 steps fills a box, and a new box can be earned every 6 hours, so up to four a day. If you miss one, up to two wait for you."),
    ("What are the box odds?", "Every box shows its odds before you open or buy it, including the chance of rare gold, diamond and secret toys. Paid boxes only give toys you don't own yet."),
    ("I got a new phone. How do I get my toys back?", "Install Pockle and sign in with the same Apple or Google account you used before. Your shelf, spares, fusions and badges come with you."),
    ("How does trading work?", "Offer a spare to a friend or post it on the public ledger. In the trade room you can accept, decline, counter, or ask for a sweeter deal. Trades are toy-for-toy, expire after 24 hours, and both players confirm before anything moves."),
    ("My steps aren't counting.", "Check that Pockle has permission to read physical activity (Android) or Motion &amp; Fitness (iPhone) in your phone's settings, and that walking is turned on in Pockle's settings."),
    ("I bought a box but didn't get it.", f"Close and reopen Pockle while connected to the internet; it will finish delivering. If it still hasn't arrived, email {mail(SUPPORT)} with the time of purchase and your collector name."),
    ("How do I report or block someone?", "Tap their collector name, then Report or Block. Blocked players can't send you trades or reactions."),
    ("Can I change my collector name?", "Yes, from your profile. Names must be 3–16 letters, numbers or underscores, and must not be your real name or offensive."),
]
SUPPORT_BODY = f"""<div class="doc-hero mint"><div class="wrap"><h1>Support</h1><p>Answers to common questions, and how to reach a real person.</p></div></div>
<div class="doc"><nav class="toc" aria-label="On this page"><a href="#faq">Questions</a><a href="#parents">For parents</a><a href="#contact">Contact us</a></nav><article>
<h2 id="faq">Common questions</h2>
<div class="faq">{''.join(f'<details><summary>{q}</summary><p>{a}</p></details>' for q, a in FAQ)}</div>
<h2 id="parents">For parents</h2>
<p>Pockle is for players 13 and older. There's no open chat: players react with preset stickers and phrases, and trades are toy-for-toy with no money involved. Purchases go through Apple or Google, so you can use Screen Time or Google Family Link to require approval.
To ask about or delete a teen's account, email {mail(PRIVACY)}.</p>
<h2 id="contact">Contact us</h2>
<div class="note"><p style="margin:0">Email {mail(SUPPORT)} with your collector name and what happened. We usually reply within two business days.</p></div>
<p>Privacy requests: {mail(PRIVACY)} · Legal notices: {mail(LEGAL)}</p>
</article></div>"""

# ---------------------------------------------------------------- Delete account
DELETE_BODY = f"""<div class="doc-hero peach"><div class="wrap"><h1>Delete your Pockle account</h1><p>How to permanently delete your account and data, from the app or without it.</p></div></div>
<div class="doc"><nav class="toc" aria-label="On this page"><a href="#app">In the app</a><a href="#email">Without the app</a><a href="#what">What gets deleted</a></nav><article>
<h2 id="app">In the app</h2>
<ol class="steps"><li>Open Pockle and tap <strong>Settings</strong> (the gear on Home).</li><li>Tap <strong>Your account</strong>, then <strong>Delete account</strong>.</li><li>Confirm the deletion request. You'll be signed out; deletion of associated cloud account data finishes within 30 days.</li></ol>
<h2 id="email">Without the app</h2>
<p>Email {mail(PRIVACY)} from the email address linked to your Apple or Google sign-in, with the subject <strong>"Delete my Pockle account"</strong> and your collector name. We'll confirm it's your account and delete it within 30 days.
Parents can request deletion of a teen's account the same way.</p>
<h2 id="what">What gets deleted, and what's kept</h2>
<table class="plain"><tr><th>Deleted</th><th>Kept, and why</th></tr>
<tr><td>Your Pockle account and sign-in link, collector name and profile, cloud saves containing toys, spares, fusions, badges, step history and saved preferences, and associated personal data held by our service providers, subject to the retention exceptions opposite.</td>
<td>Purchase and transaction records where needed for tax, accounting or fraud prevention. Apple and Google retain their own store records under their policies.</td></tr></table>
<p>Account deletion does not automatically erase local app data on every phone you have used. Local copies may remain until you remove the app or clear its stored data. Removing the app alone does not request deletion of your cloud account.</p>
<div class="note">Deleting is permanent. Your toys can't be restored, and purchases aren't refunded except where the law requires. If you only want to stop, you can sign out or turn off walking and notifications instead.</div>
</article></div>"""


def build():
    if OUT.exists():
        shutil.rmtree(OUT)
    shutil.copytree(ROOT / "assets", OUT / "assets")
    shutil.copyfile(HOME_SOURCE, OUT / "index.html")
    doc("privacy", "Privacy Policy", "How Pockle collects, uses and protects your information.", "moon",
        f"Last updated {PRIVACY_UPDATED}. This policy explains what {COMPANY} collects when you play Pockle, and why.", PRIV, "/privacy/")
    doc("terms", "Terms of Service", "The rules for playing Pockle.", "", f"Last updated {UPDATED}. The rules for playing Pockle.", TERMS, "/terms/")
    page("support", "Support · Pockle", "Help with Pockle: walking, boxes, trading, purchases and accounts.", SUPPORT_BODY, "/support/")
    page("delete-account", "Delete your account · Pockle", "How to delete your Pockle account and data.", DELETE_BODY)
    page("_404", "Page not found · Pockle", "This page doesn't exist.",
         '<div class="doc-hero"><div class="wrap"><h1>This page wandered off.</h1><p>It might have been traded away. <a href="/">Back to Pockle</a></p></div></div>')
    (OUT / "_404" / "index.html").rename(OUT / "404.html"); (OUT / "_404").rmdir()
    (OUT / "robots.txt").write_text("User-agent: *\nAllow: /\nSitemap: https://playpockle.com/sitemap.xml\n")
    urls = ["", "support/", "privacy/", "terms/", "delete-account/"]
    (OUT / "sitemap.xml").write_text('<?xml version="1.0" encoding="UTF-8"?>\n<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">'
        + "".join(f"<url><loc>https://playpockle.com/{u}</loc></url>" for u in urls) + "</urlset>\n")


if __name__ == "__main__":
    build()
    print("built", sum(1 for _ in OUT.rglob("*") if _.is_file()), "files")
