function Nav({navItems, activeCategory, clickHandler}) {
  return(<nav aria-label="Categories">
    <ul>
      {navItems.map(item =>
        <li key={item}>
          <a onClick={clickHandler} href="#" aria-current={item === activeCategory ? "page" : undefined}>{item}</a>
        </li>
      )}
    </ul>
    </nav>
  );
}