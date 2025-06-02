# 💼 **VentaSoft HA - Sistema de Ventas y Logística**

---

## 📌 **Tabla de Contenidos**

* [📋 Descripción](#-descripción)
* [✨ Características Principales](#-características-principales)
* [🛠️ Instalación](#-instalación)
* [⚙️ Flujo de Trabajo Básico](#-flujo-de-trabajo-básico)
* [🏗️ Arquitectura](#-arquitectura)
* [🔧 Configuración](#-configuración)
* [🧪 Pruebas](#-pruebas)
* [🫰 Solución de Problemas](#-solución-de-problemas)
* [🔐 Seguridad](#-seguridad)
* [🧰 Versiones](#-versiones)
* [🧑‍💻 Desarrollo](#-desarrollo)
* [👥 Equipo](#-equipo)
* [📚 Referencias Bibliográficas](#-referencias-bibliográficas)

---

## 📋 **Descripción**

**VentaSoft HA** es un sistema integral de gestión de ventas y logística diseñado específicamente para pequeñas y medianas empresas que requieren soluciones tecnológicas para optimizar sus procesos comerciales y facilitar la gestión de sus operaciones diarias.

El sistema automatiza tareas clave como:

* Creación de clientes y usuarios
* Control de inventario
* Facturación
* Registro de ventas
* Generación de reportes

### 🎯 **Objetivos**

* Automatizar procesos comerciales
* Facilitar la administración de clientes, productos y usuarios
* Proporcionar control completo del inventario
* Ofrecer herramientas de análisis mediante dashboard y reportes
* Mejorar la toma de decisiones estratégicas

---

## ✨ **Características Principales**

### 📊 **Dashboard Interactivo**

* Gráficos dinámicos de ventas
* Indicadores clave de rendimiento (KPIs)
* Reportes en tiempo real
* Visualización de tendencias
* Filtros avanzados (fecha, categoría, producto)

### 📝 **Gestión de Productos**

* Catálogo completo de productos
* Control de stock y alertas de inventario
* Categorización y etiquetado
* Gestión de precios y descuentos por fecha de nacimiento
* Importación/exportación de datos
* Búsqueda avanzada y filtrado

### 👥 **Gestión de Clientes**

* Base de datos completa
* Historial de compras
* Segmentación de clientes
* Notificaciones automáticas

### 💰 **Sistema de Ventas**

* Proceso de venta simplificado
* Descuentos por fecha de nacimiento
* Facturación
* Registro de ventas por usuario

### 📦 **Control de Inventario**

* Seguimiento en tiempo real
* Alertas de stock mínimo
* Gestión de proveedores
* Registro de entradas y salidas
* Valoración y ajustes de inventario

### 📈 **Reportes y Análisis**

* Reportes diarios, semanales y mensuales
* Análisis de rentabilidad
* Estadísticas de clientes
* Exportación a Excel, PDF y CSV

---

## 🚀 **Requisitos Previos**

* 💻 Windows 10/11
* 🧱 .NET Framework 4.7.2 o superior
* 📃 SQL Server 2019 o superior
* 💾 Visual Studio 2019 o superior
* 🧠 4GB de RAM mínimo
* 📀 500MB de espacio libre en disco

---

## 🛠️ **Instalación**

1. Clona el repositorio:
   `git clone https://github.com/hx4m099/VentasoftHA.git`
2. Abre la solución en Visual Studio
3. Restaura los paquetes NuGet
4. Compila y ejecuta

---

## ⚙️ **Flujo de Trabajo Básico**

### 📦 Registro de Productos

1. Accede al módulo "Productos"
2. Haz clic en "Nuevo Producto"
3. Completa la información
4. Guarda los cambios

### 👥 Registro de Clientes

1. Accede al módulo "Clientes"
2. Haz clic en "Nuevo Cliente"
3. Ingresa los datos
4. Guarda los cambios

### 💳 Proceso de Venta

1. Accede al módulo "Ventas"
2. Selecciona "Nueva Venta"
3. Busca y selecciona el cliente
4. Agrega productos al carrito
5. Aplica descuentos si aplica
6. Finaliza la venta y genera factura

### 📊 Generación de Reportes

1. Accede al módulo "Reportes"
2. Selecciona el tipo de reporte
3. Aplica los filtros deseados
4. Genera y visualiza
5. Exporta si es necesario

---

## 🏗️ **Arquitectura**

### 🧱 Capas de la Aplicación:

#### 🎨 Presentation Layer

* Interfaz de usuario (Windows Forms)
* Controladores de eventos
* Validación de entrada
* Notificaciones visuales

#### ⚙️ Business Logic Layer (BLL)

* Lógica de negocio
* Reglas de validación
* Cálculos y procesamiento
* Gestión de transacciones

#### 📂 Data Access Layer (DAL)

* Acceso a base de datos
* Operaciones CRUD
* Procedimientos almacenados
* Manejo de conexiones

#### 📦 Entity Layer

* Modelos de datos
* Objetos de transferencia
* Mapeo objeto-relacional

---

## 🔧 **Configuración**

### 🔌 Configuración de Base de Datos

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=VentaSoftDB;Trusted_Connection=True;"
  }
}
```

### 📂 Estructura de Carpetas

```
VentaSoftHA/
├── GUI/                 # Capa de presentación
├── Logica/              # Lógica de negocio
├── Datos/               # Acceso a datos
├── Entidades/           # Modelos y entidades

```

---

## 🧪 **Pruebas**

* Se realizaron pruebas de integración entre las capas.
* Verificación de casos de uso principales: registro de productos, clientes, ventas, generación de reportes.
* Se incluyeron pruebas unitarias para lógica de negocio.
* Pruebas funcionales realizadas con usuarios reales en entorno de pruebas.

---

## 🫰 **Solución de Problemas**

### 🧩 Problemas comunes y soluciones

* **Error de conexión a base de datos:**

  * Verifica que el servidor SQL esté corriendo.
  * Asegúrate de que la cadena de conexión en `app.config` esté correctamente configurada.

* **Faltan paquetes NuGet:**

  * Ejecuta `Rebuild Solution` después de restaurar paquetes NuGet.

* **Errores de ejecución al cargar formularios:**

  * Verifica que todas las dependencias de cada capa estén referenciadas correctamente.

---

## 🔐 **Seguridad**

* Autenticación de usuarios con credenciales encriptadas
* Autorización por roles (Administrador, Vendedor)
* Validación de entrada para prevenir ataques de inyección SQL
* Gestión segura de contraseñas mediante hashing

---

## 🧰 **Versiones**

* **v1.0.0** - Versión inicial con módulos de ventas, productos, clientes y reportes básicos
* **v1.1.0** - Se añadieron funciones de exportación, validaciones mejoradas y dashboard

---

## 🧑‍💻 **Desarrollo**

El proyecto se desarrolla en **C#** bajo el paradigma de **Programación Orientada a Objetos**, utilizando arquitectura en capas. Se prevé incluir conexión a base de datos SQL Server y migrar hacia una arquitectura más escalable basada en microservicios para futuras versiones.

---

## 👥 **Equipo**

### 👨‍💻 Desarrolladores

* **Amin Pineda**
  📧 [ajosepineda@unicesar.edu.co](mailto:ajosepineda@unicesar.edu.co)

* **Héctor López**
  📧 [hricardolopez@unicesar.edu.co](mailto:hricardolopez@unicesar.edu.co)

* **Duan Pineda**
  📧 [dapineda@unicesar.edu.co](mailto:dapineda@unicesar.edu.co)

### 🧑‍🏫 Supervisión Académica

**John Jairo Patiño Vanegas - Docente**

* Programación de Computadores III
* Universidad Popular del Cesar

### 📱 Contacto

* Email: *ventasoftha@gmail.com*
* Teléfono: +57 300667792

---

## 📚 **Referencias Bibliográficas**

* [Microsoft .NET Desktop Development](https://learn.microsoft.com/es-es/dotnet/desktop/wpf/xaml/)
* [Tutoriales de desarrollo WPF (YouTube)](https://www.youtube.com/watch?v=zK5mk_zmqqs&ab_channel=INFORMATICONFIG)
* [Guía de WPF y XAML](https://wpf-tutorial.com/es/5/xaml/que-es-xaml/)
* [Desarrollo de aplicaciones con Telegram Bots](https://core.telegram.org/bots)




