const grid = document.querySelector('.screen-grid');
const themeButtons = [...document.querySelectorAll('[data-theme].direction')];
themeButtons.forEach(button => button.addEventListener('click', () => {
  grid.dataset.theme = button.dataset.theme;
  themeButtons.forEach(b => { b.classList.toggle('active', b === button); b.setAttribute('aria-pressed', String(b === button)); });
}));

const modal = document.querySelector('#offer-modal');
const tradeModal = document.querySelector('#trade-modal');
let returnFocus;
function openModal(target, button) {
  returnFocus = button;
  target.hidden = false;
  target.querySelector('.close-sheet').focus();
}
function closeModal(target) { target.hidden = true; returnFocus?.focus(); }
[modal, tradeModal].forEach(target => {
  target.querySelector('.close-sheet').addEventListener('click', () => closeModal(target));
  target.addEventListener('click', e => { if (e.target === target) closeModal(target); });
});
document.addEventListener('keydown', e => {
  const open = [modal, tradeModal].find(target => !target.hidden);
  if (!open) return;
  if (e.key === 'Escape') closeModal(open);
  if (e.key === 'Tab') {
    const controls = [...open.querySelectorAll('button:not([disabled]),select,input:not([disabled])')];
    const first = controls[0], last = controls[controls.length - 1];
    if (e.shiftKey && document.activeElement === first) { e.preventDefault(); last.focus(); }
    else if (!e.shiftKey && document.activeElement === last) { e.preventDefault(); first.focus(); }
  }
});
document.querySelectorAll('[data-offer]').forEach(button => button.addEventListener('click', () => {
  const id = button.dataset.offer;
  document.querySelector('#offer-title').textContent = id[0].toUpperCase() + id.slice(1) + ' collection box';
  document.querySelector('#offer-cover').src = 'assets/' + id + '-cover.png';
  openModal(modal, button);
}));

// All entries are static design fixtures. There are no accounts, network calls, reservations or transfers.
const tradeExamples = {
  moon: { owner: 'Maple', receive: ['Moon Jelly', 'Pip · Common', 'pip-moon-pearl.png'], give: ['Mint Soft', 'Pip · Common', 'pip-mint-mochi.png'] },
  kelp: { owner: 'Fern', receive: ['Kelp Mint', 'Wally · Common', 'wally.png'], give: ['Mochi', 'Fox · Common', 'fox.png'] },
  gold: { owner: 'Sol', receive: ['Polished Gold', 'Pip · Rare', 'pip-coated-metallic.png'], give: ['Starlight Glow', 'Pip · Epic', 'pip-glow-jelly.png'] }
};
const collectionFilter = document.querySelector('#trade-collection');
const rarityFilter = document.querySelector('#trade-rarity');
const marketOffers = [...document.querySelectorAll('.market-offer')];
function filterTrades() {
  let shown = 0;
  marketOffers.forEach(offer => {
    offer.hidden = !(collectionFilter.value === 'all' || offer.dataset.collection === collectionFilter.value)
      || !(rarityFilter.value === 'all' || offer.dataset.rarity === rarityFilter.value);
    if (!offer.hidden) shown++;
  });
  document.querySelector('#market-count').textContent = shown + ' sample offer' + (shown === 1 ? '' : 's') + ' · filter by the offered toy';
  document.querySelector('.market-empty').hidden = shown > 0;
}
collectionFilter.addEventListener('change', filterTrades);
rarityFilter.addEventListener('change', filterTrades);
let currentTrade, acceptedTrade = false;
function updateAcceptance() {
  const agreed = document.querySelector('#preview-agree').checked;
  const yours = document.querySelector('#your-confirmation');
  yours.textContent = acceptedTrade ? 'Accepted · simulation' : agreed ? 'Terms agreed · preview' : 'Review the fixed terms';
  yours.classList.toggle('confirmed', agreed);
  const confirm = document.querySelector('#preview-confirm');
  confirm.disabled = !agreed || acceptedTrade;
  confirm.textContent = acceptedTrade ? 'Offer accepted · simulation' : 'Simulate accepting offer';
  document.querySelector('#preview-agree').disabled = acceptedTrade;
  document.querySelector('#trade-simulation-status').textContent = acceptedTrade
    ? 'Simulation complete. The real server would atomically recheck the published offer and both copies, then record the exchange. The owner need not be online.'
    : 'The owner already confirmed this offer when publishing. They do not need to be online when you accept. No toys move in this preview.';
  document.querySelector('#mock-trade-receipt').hidden = !acceptedTrade;
  document.querySelector('#mock-trade-result').textContent = acceptedTrade
    ? 'Example: you gave 1 ' + currentTrade.give[0] + ' spare and received 1 ' + currentTrade.receive[0] + '. No toys were traded here.' : '';
}
document.querySelectorAll('[data-trade]').forEach(button => button.addEventListener('click', () => {
  const offer = tradeExamples[button.dataset.trade];
  if (!offer) return;
  document.querySelector('#trade-owner').textContent = 'Sample public listing from ' + offer.owner + ' · one spare for one toy';
  document.querySelector('#owner-confirmation-label').textContent = offer.owner + '’s side';
  for (const side of ['receive', 'give']) {
    const [name, rarity, art] = offer[side];
    const picture = document.querySelector('#trade-' + side + '-image');
    picture.src = 'assets/' + art; picture.alt = name + ', ' + rarity;
    document.querySelector('#trade-' + side + '-name').textContent = name;
    document.querySelector('#trade-' + side + '-tier').textContent = rarity;
  }
  document.querySelector('#trade-availability').textContent = 'This example assumes ' + offer.owner + ' has 1 ' + offer.receive[0]
    + ' spare available, and you have 1 ' + offer.give[0] + ' spare available. These counts are illustrative, not connected to an account.';
  currentTrade = offer; acceptedTrade = false;
  document.querySelector('#preview-agree').checked = false;
  updateAcceptance();
  openModal(tradeModal, button);
}));
document.querySelector('#preview-agree').addEventListener('change', updateAcceptance);
document.querySelector('#preview-confirm').addEventListener('click', () => {
  if (!currentTrade || !document.querySelector('#preview-agree').checked || acceptedTrade) return;
  acceptedTrade = true; updateAcceptance();
});
let toastTimer;
document.querySelectorAll('[data-demo]').forEach(button => button.addEventListener('click', () => {
  const toast = document.querySelector('.toast'); toast.textContent = button.dataset.demo; toast.hidden = false;
  clearTimeout(toastTimer); toastTimer = setTimeout(() => toast.hidden = true, 6500);
}));
