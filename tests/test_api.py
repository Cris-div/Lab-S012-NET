"""Pruebas HTTP reales. Python 3 sin paquetes externos. Usa datos temporales propios."""
import argparse
from datetime import datetime, timezone
import html
import json
from pathlib import Path
import secrets
import urllib.error
import urllib.parse
import urllib.request

parser = argparse.ArgumentParser()
parser.add_argument("base_url", help="URL de la API sin /swagger")
parser.add_argument("--allow-local", action="store_true", help="Permite localhost; el informe se marca LOCAL")
parser.add_argument("--output", default="artifacts/pruebas")
parser.add_argument("--origin", default="http://localhost:3000", help="El origen exacto configurado en Cors__AllowedOrigin")
args = parser.parse_args()
base = args.base_url.rstrip("/")
uri = urllib.parse.urlparse(base)
local = uri.hostname in ("localhost", "127.0.0.1", "::1")
if uri.scheme not in ("http", "https") or not uri.hostname or uri.username or uri.password:
    parser.error("Usa una URL HTTP(S) sin credenciales.")
if local and not args.allow_local:
    parser.error("Para la revisión local agrega --allow-local. La entrega exige repetir sobre la URL publicada.")
if not local and uri.scheme != "https":
    parser.error("La API publicada debe usar HTTPS.")

rows = []
owners, pets = [], []


def call(name, method, path, expected, payload=None, check=None, headers=None):
    data = None if payload is None else json.dumps(payload).encode()
    request_headers = {"Content-Type": "application/json", **(headers or {})}
    req = urllib.request.Request(base + path, data=data, headers=request_headers, method=method)
    try:
        response = urllib.request.urlopen(req, timeout=180)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        text = response.read().decode()
        status, received_headers = response.code, dict(response.headers)
    try:
        parsed = json.loads(text) if text else None
    except json.JSONDecodeError:
        parsed = text
    ok = status == expected
    detail = ""
    if ok and check:
        try:
            assert check(parsed, received_headers), "El contenido o los encabezados no cumplen lo esperado."
        except (AssertionError, KeyError, TypeError) as error:
            ok, detail = False, str(error)
    rows.append(dict(name=name, method=method, url=base + path, expected=expected, status=status,
                     passed=ok, request=payload, requestHeaders=headers, response=parsed,
                     headers=received_headers, detail=detail))
    print(f"{'OK' if ok else 'ERROR'} {status}/{expected} {name}")
    if not ok:
        raise AssertionError(f"{name}: {status} {text} {detail}")
    return parsed


def owner(document, **extra):
    return dict(nombre="Prueba temporal S12", documento=document, telefono="999888777", **extra)


def pet(owner_id, **extra):
    return dict(nombre="Mascota temporal S12", especie="Gato", edad=2, peso=4.25,
                propietarioId=owner_id, **extra)


failure = None
try:
    call("Swagger disponible", "GET", "/swagger/index.html", 200,
         check=lambda data, _: 'id="swagger-ui"' in data)
    # OpenAPI enumera exactamente las 11 operaciones CRUD y la ruta anidada.
    call("OpenAPI público", "GET", "/openapi/v1.json", 200,
         check=lambda data, _: sum(len(v) for v in data["paths"].values()) == 11)
    call("Listar propietarios", "GET", "/api/propietarios", 200, check=lambda d, _: isinstance(d, list))
    call("Listar mascotas", "GET", "/api/mascotas", 200, check=lambda d, _: isinstance(d, list))
    # Dos documentos aleatorios; no se modifica ningún registro de ejemplo.
    docs = set()
    while len(docs) < 2:
        docs.add(str(secrets.randbelow(90000000) + 10000000))
    doc1, doc2 = docs
    p = call("Crear propietario", "POST", "/api/propietarios", 201, owner(doc1),
             check=lambda d, h: h.get("Location", "").endswith(f'/api/Propietarios/{d["id"]}'))
    pid = p["id"]
    owners.append(pid)
    p2 = call("Crear segundo propietario", "POST", "/api/propietarios", 201, owner(doc2))
    pid2 = p2["id"]
    owners.append(pid2)
    call("Consultar propietario creado", "GET", f"/api/propietarios/{pid}", 200,
         check=lambda d, _: d["documento"] == doc1)
    call("Documento duplicado POST", "POST", "/api/propietarios", 409, owner(doc1))
    call("Documento duplicado PUT", "PUT", f"/api/propietarios/{pid2}", 409, owner(doc1, id=pid2))
    call("Required automático", "POST", "/api/propietarios", 400, {"documento": doc1, "telefono": "999888777"})
    call("StringLength automático", "POST", "/api/propietarios", 400,
         {"nombre": "X" * 101, "documento": doc1, "telefono": "999888777"})
    call("Formato de documento", "POST", "/api/propietarios", 400, owner("ABCDEFGH"))
    call("Id autogenerado propietario", "POST", "/api/propietarios", 400, owner(doc1, id=999))
    call("Id propietario no coincide", "PUT", f"/api/propietarios/{pid}", 400, owner(doc1, id=pid2))
    call("Actualizar propietario", "PUT", f"/api/propietarios/{pid}", 204,
         {**owner(doc1, id=pid), "nombre": "Propietario actualizado S12"})
    call("Verificar actualización propietario", "GET", f"/api/propietarios/{pid}", 200,
         check=lambda d, _: d["nombre"] == "Propietario actualizado S12")
    call("Ruta anidada vacía", "GET", f"/api/propietarios/{pid}/mascotas", 200, check=lambda d, _: d == [])
    m = call("Crear mascota solo con FK", "POST", "/api/mascotas", 201, pet(pid),
             check=lambda d, h: h.get("Location", "").endswith(f'/api/Mascotas/{d["id"]}'))
    mid = m["id"]
    pets.append(mid)
    call("Consultar mascota con propietario y sin ciclo", "GET", f"/api/mascotas/{mid}", 200,
         check=lambda d, _: d["propietario"]["id"] == pid and "mascotas" not in d["propietario"])
    call("Filtros combinados", "GET", f"/api/mascotas?propietarioId={pid}&buscar=temporal&especie=Gato", 200,
         check=lambda d, _: len(d) == 1 and d[0]["id"] == mid)
    call("Filtros sin coincidencias", "GET", f"/api/mascotas?propietarioId={pid}&especie=Perro", 200,
         check=lambda d, _: d == [])
    call("Filtro inválido", "GET", "/api/mascotas?propietarioId=0", 400)
    call("Ruta anidada con mascota", "GET", f"/api/propietarios/{pid}/mascotas", 200,
         check=lambda d, _: len(d) == 1 and d[0]["id"] == mid)
    call("Eliminar propietario con dependientes", "DELETE", f"/api/propietarios/{pid}", 409)
    call("FK inexistente POST", "POST", "/api/mascotas", 400, pet(2147483647))
    call("FK inexistente PUT", "PUT", f"/api/mascotas/{mid}", 400, pet(2147483647, id=mid))
    call("Range peso automático", "POST", "/api/mascotas", 400, {**pet(pid), "peso": -1})
    call("Range edad automático", "POST", "/api/mascotas", 400, {**pet(pid), "edad": 51})
    call("Required mascota automático", "POST", "/api/mascotas", 400, {**pet(pid), "nombre": " "})
    call("Id autogenerado mascota", "POST", "/api/mascotas", 400, pet(pid, id=999))
    call("Id mascota no coincide", "PUT", f"/api/mascotas/{mid}", 400, pet(pid, id=mid + 1))
    call("Actualizar y reasignar mascota", "PUT", f"/api/mascotas/{mid}", 204,
         {**pet(pid2, id=mid), "nombre": "Mascota actualizada S12"})
    call("Verificar actualización mascota", "GET", f"/api/mascotas/{mid}", 200,
         check=lambda d, _: d["nombre"] == "Mascota actualizada S12" and d["propietarioId"] == pid2)
    call("Eliminar mascota", "DELETE", f"/api/mascotas/{mid}", 204)
    pets.remove(mid)
    call("Mascota inexistente GET", "GET", f"/api/mascotas/{mid}", 404)
    call("Mascota inexistente PUT", "PUT", f"/api/mascotas/{mid}", 404, pet(pid, id=mid))
    call("Mascota inexistente DELETE", "DELETE", f"/api/mascotas/{mid}", 404)
    call("Eliminar propietario", "DELETE", f"/api/propietarios/{pid}", 204)
    owners.remove(pid)
    call("Propietario inexistente GET", "GET", f"/api/propietarios/{pid}", 404)
    call("Propietario inexistente PUT", "PUT", f"/api/propietarios/{pid}", 404, owner(doc1, id=pid))
    call("Propietario inexistente DELETE", "DELETE", f"/api/propietarios/{pid}", 404)
    call("Padre inexistente en ruta anidada", "GET", f"/api/propietarios/{pid}/mascotas", 404)
    call("CORS origen permitido", "OPTIONS", "/api/mascotas", 204,
         headers={"Origin": args.origin, "Access-Control-Request-Method": "POST"},
         check=lambda _, h: h.get("Access-Control-Allow-Origin") == args.origin)
    call("CORS origen ajeno", "OPTIONS", "/api/mascotas", 204,
         headers={"Origin": "https://origen-ajeno.example", "Access-Control-Request-Method": "POST"},
         check=lambda _, h: "Access-Control-Allow-Origin" not in h)
except Exception as error:
    failure = str(error)
finally:
    for mid in pets:
        try:
            call("Limpieza mascota temporal", "DELETE", f"/api/mascotas/{mid}", 204)
        except Exception as error:
            failure = failure or str(error)
    for pid in owners:
        try:
            call("Limpieza propietario temporal", "DELETE", f"/api/propietarios/{pid}", 204)
        except Exception as error:
            failure = failure or str(error)
    directory = Path(args.output)
    directory.mkdir(parents=True, exist_ok=True)
    environment = "LOCAL — no constituye evidencia de nube" if local else "URL REMOTA — verificar proveedor y BD en la entrega"
    report = dict(baseUrl=base, environment=environment, date=datetime.now(timezone.utc).isoformat(),
                  passed=not failure, failure=failure, tests=rows)
    (directory / "resultados.json").write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
    sections = []
    for i, row in enumerate(rows, 1):
        request = json.dumps(row["request"], ensure_ascii=False, indent=2)
        response = json.dumps(row["response"], ensure_ascii=False, indent=2)
        sections.append(f'<article><h2>{i}. {html.escape(row["name"])}</h2>'
                        f'<p>{html.escape(row["method"] + " " + row["url"])}</p>'
                        f'<p>HTTP {row["status"]} | Esperado {row["expected"]} | {"OK" if row["passed"] else "ERROR"}</p>'
                        f'<h3>Solicitud</h3><pre>{html.escape(request)}</pre>'
                        f'<h3>Respuesta</h3><pre>{html.escape(response)}</pre></article>')
    (directory / "resultados.html").write_text(
        '<!doctype html><html lang="es"><meta charset="utf-8"><title>Pruebas Veterinaria API</title>'
        '<style>body{font:16px system-ui;max-width:1000px;margin:32px auto;padding:0 20px;color:#17212b}'
        'article{border:1px solid #ccd4dc;padding:20px;margin:24px 0;break-inside:avoid}pre{white-space:pre-wrap;overflow-wrap:anywhere}'
        'h1{font-size:28px}h2{font-size:21px}h3{font-size:17px}</style>'
        f'<h1>Pruebas Veterinaria API</h1><p>{html.escape(environment)}</p>'
        f'<p>{html.escape(base)} | {html.escape(report["date"])}</p>'
        f'<p>{sum(r["passed"] for r in rows)}/{len(rows)} pruebas correctas.</p>'
        + "".join(sections) + '</html>', encoding="utf-8")
if failure:
    raise SystemExit(f"FALLO: {failure}")
print(f"Informe: {directory.resolve()}")
