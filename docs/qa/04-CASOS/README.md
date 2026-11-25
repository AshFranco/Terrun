# Casos del motor calculados a mano (LLD §9)

Cada caso MT del LLD §9 tiene aquí su entrada, el cálculo paso a paso y el resultado esperado. La prueba automatizada que lo comprueba lleva el atributo `[Trait("Case", "MT-NN")]` y se indica en cada archivo. Si una prueba y un cálculo no coinciden, manda el cálculo: se corrige el código o se abre un defecto.

## Convenciones

- **Coordenadas inventadas** (estándares §6): origen en `lat 10.0, lon -60.0`, en medio del mar Caribe. Los puntos avanzan hacia el norte por el meridiano, porque ahí la distancia de Haversine es exactamente `R × Δφ` y el cálculo a mano es directo.
- **Radio de la Tierra:** `R = 6.371.008,8 m` (radio medio de la IUGG). Para avanzar `d` metros al norte: `Δlat = d / R × 180 / π` grados.
- **Parámetros:** los de la semilla de `system_setting` (LLD §2.4): `anticheat.max_accuracy_m = 30`, `anticheat.max_jump_m = 200`, `anticheat.jump_window_s = 5`, `anticheat.max_speed_kmh = 25`, `anticheat.min_pace_s_per_km = 150`, `anticheat.max_invalid_distance_pct = 20` y `activity.auto_pause_speed_mps = 0,5` (umbral de "en movimiento").
- **Velocidad de un tramo:** `v = d / Δt` en m/s; en km/h, `v × 3,6`.

## Casos

| Caso | Historia | Archivo | Prueba |
|---|---|---|---|
| MT-01 | CTZ-31a | [MT-01.md](MT-01.md) | `TrackCleanerTests.Haversine_PointsOneKilometerApart_Returns1000Meters` |
| MT-02 | CTZ-31a | [MT-02.md](MT-02.md) | `TrackCleanerTests.Clean_PointWithAccuracyOf45Meters_IsDiscardedAndAddsNoDistance` |
| MT-03 | CTZ-31a | [MT-03.md](MT-03.md) | `TrackCleanerTests.Clean_LegAt40KmPerHour_IsInvalidAndAddsNoDistance` |
| MT-04 | CTZ-31a | [MT-04.md](MT-04.md) | `AntiCheatJudgeTests.Judge_PaceOf2m10sPerKmFor20Minutes_SendsTheActivityToReview` |
| MT-09 | CTZ-30 | — (no es un cálculo) | `ActivityUploadTests` con `[Trait("Case", "MT-09")]` |
| MT-14 | CTZ-31a | [MT-14.md](MT-14.md) | `TrackCleanerTests.Clean_PauseOf10MinutesBetweenSegments_AddsNoDistanceAndNoMovingTime` |
