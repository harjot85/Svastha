create table ping (
    id bigserial primary key,
    message text not null,
    created_at timestamptz not null default now()
);
