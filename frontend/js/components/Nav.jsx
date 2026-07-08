function Nav({navItems, activeCategory, clickHandler}) {
  /*
  <ul>
      <li><a href="#" data-name="top" aria-current="page">top</a></li>
      <li><a href="#" data-name="general">general</a></li>
      <li><a href="#" data-name="business">business</a></li>
      <li><a href="#" data-name="technology">technology</a></li>
      <li><a href="#" data-name="science">science</a></li>
      <li><a href="#" data-name="health">health</a></li>
      <li><a href="#" data-name="sports">sports</a></li>
      <li><a href="#" data-name="entertainment">entertainment</a></li>
    </ul>
  </nav>
  */
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