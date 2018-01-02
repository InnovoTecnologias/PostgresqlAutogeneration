# PostgresqlAutogeneration
Function generator for PostgreSQL

You just copy-paste the table definition from pgAdmin and then press Generate.

For example, this one:

			-- Table: proyectos.equipo
			-- DROP TABLE proyectos.equipo;

			CREATE TABLE proyectos.equipo
			(
				id integer NOT NULL DEFAULT nextval('proyectos.equipo_id_seq'::regclass),
				idtipo integer NOT NULL,
				nombre character varying(250) COLLATE pg_catalog.""default"" NOT NULL,
				numero integer NOT NULL,
				usuario character varying(250) COLLATE pg_catalog.""default"" NOT NULL,
				cpu character varying(250) COLLATE pg_catalog.""default"" NOT NULL,
				CONSTRAINT equipo_pkey PRIMARY KEY (id),
				CONSTRAINT fk_equipo_idtipo FOREIGN KEY (idtipo)
					REFERENCES proyectos.equipo_tipo (id) MATCH SIMPLE
					ON UPDATE NO ACTION
					ON DELETE NO ACTION
			)
			WITH (
				OIDS = FALSE
			)
			TABLESPACE pg_default;

			ALTER TABLE proyectos.equipo
				OWNER to postgres;

The script to create the add, modify, delete and get functions will be ready to copy. You can then run it in the Query Tool of pgAdmin as it is or customize it.