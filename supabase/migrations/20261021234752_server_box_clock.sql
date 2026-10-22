create table pockle_private.box_collections (
 id text primary key, position integer not null unique, collectible_ids text[] not null, enabled boolean not null default true
);
-- Approved current beta roster. These are collection reservations, not finite-edition production grants.
insert into pockle_private.box_collections(id,position,collectible_ids) values
('pip',1,array['pip.peach-jelly','pip.moon-pearl','pip.gold-confetti','pip.mint-mochi','pip.velvet-flock','pip.boucle-plush','pip.matte-vinyl','pip.gloss-vinyl','pip.coated-metallic','pip.glow-jelly']),
('fox',2,array['fox.clear-jelly','fox.pearl-jelly','fox.confetti-gel','fox.mochi-foam','fox.velvet-flock','fox.boucle-plush','fox.matte-vinyl','fox.gloss-vinyl','fox.coated-metallic','fox.glow-jelly']),
('axolotl',3,array['axolotl.clear-jelly','axolotl.pearl-jelly','axolotl.confetti-gel','axolotl.mochi-foam','axolotl.velvet-flock','axolotl.boucle-plush','axolotl.matte-vinyl','axolotl.gloss-vinyl','axolotl.coated-metallic','axolotl.glow-jelly']),
('firb',4,array['firb.clear-jelly','firb.pearl-jelly','firb.confetti-gel','firb.mochi-foam','firb.velvet-flock','firb.boucle-plush','firb.matte-vinyl','firb.gloss-vinyl','firb.coated-metallic','firb.glow-jelly']),
('coco',5,array['coco.clear-jelly','coco.pearl-jelly','coco.confetti-gel','coco.mochi-foam','coco.velvet-flock','coco.boucle-plush','coco.matte-vinyl','coco.gloss-vinyl','coco.coated-metallic','coco.glow-jelly']),
('snowy',6,array['snowy.clear-jelly','snowy.pearl-jelly','snowy.confetti-gel','snowy.mochi-foam','snowy.velvet-flock','snowy.boucle-plush','snowy.matte-vinyl','snowy.gloss-vinyl','snowy.coated-metallic','snowy.glow-jelly']),
('ginger',7,array['ginger.clear-jelly','ginger.pearl-jelly','ginger.confetti-gel','ginger.mochi-foam','ginger.velvet-flock','ginger.boucle-plush','ginger.matte-vinyl','ginger.gloss-vinyl','ginger.coated-metallic','ginger.glow-jelly']),
('reiny',8,array['reiny.clear-jelly','reiny.pearl-jelly','reiny.confetti-gel','reiny.mochi-foam','reiny.velvet-flock','reiny.boucle-plush','reiny.matte-vinyl','reiny.gloss-vinyl','reiny.coated-metallic','reiny.glow-jelly']),
('guin',9,array['guin.clear-jelly','guin.pearl-jelly','guin.confetti-gel','guin.mochi-foam','guin.velvet-flock','guin.boucle-plush','guin.matte-vinyl','guin.gloss-vinyl','guin.coated-metallic','guin.glow-jelly']),
('bundy',10,array['bundy.clear-jelly','bundy.pearl-jelly','bundy.confetti-gel','bundy.mochi-foam','bundy.velvet-flock','bundy.boucle-plush','bundy.matte-vinyl','bundy.gloss-vinyl','bundy.coated-metallic','bundy.glow-jelly']),
('wally',11,array['wally.clear-jelly','wally.pearl-jelly','wally.confetti-gel','wally.mochi-foam','wally.velvet-flock','wally.boucle-plush','wally.matte-vinyl','wally.gloss-vinyl','wally.coated-metallic','wally.glow-jelly']);
create table pockle_private.box_wallets (
 player_id uuid primary key references pockle_private.players(id), next_box_at timestamptz not null default now(),
 day date not null default (now() at time zone 'UTC')::date, earned_today integer not null default 0 check(earned_today between 0 and 4),
 step_credit bigint not null default 0 check(step_credit between 0 and 100000), rotation_position integer not null default 0
);
create table pockle_private.walking_counters (
 player_id uuid primary key references pockle_private.players(id), device_id uuid not null, boot text not null,
 sequence bigint not null, sensor_total bigint not null, observed_at timestamptz not null default now()
);
create table pockle_private.box_reservations (
 id uuid primary key default gen_random_uuid(), player_id uuid not null references pockle_private.players(id),
 collection_id text not null references pockle_private.box_collections(id), earned_at timestamptz not null default now(),
 status text not null default 'banked' check(status in ('banked','opened')), opened_at timestamptz,
 evidence text not null default 'client_reported_beta' check(evidence='client_reported_beta')
);
create index box_reservations_player on pockle_private.box_reservations(player_id,status,earned_at);
do $$ declare t text; begin
 foreach t in array array['box_collections','box_wallets','walking_counters','box_reservations'] loop
 execute format('alter table pockle_private.%I enable row level security',t);
 execute format('revoke all on pockle_private.%I from public,anon,authenticated',t);
 execute format('grant select,insert,update,delete on pockle_private.%I to service_role',t);
 end loop;
end $$;

